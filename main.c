/* -*- mode: c; indent-tabs-mode: nil; c-basic-offset: 3; -*- */
/* vim: set expandtab shiftwidth=3 softtabstop=3 : */
/*
 *	Copyright (c) 2007 Lexmark International, Inc.
 *	All rights reserved.  Proprietary and confidential.
 *
 * File: main.c (module)
 *
 *
 * Description:
 *
 * 	This module is the "main" entry point for pdl standalone simulations.
 *
 *	Edit history:
 *
 *	When		Who	Comment
 *	---------	---	----------------------------------------------
 *      16-feb-07       rivers  Added hbp, hex support as well as use include
 *                              files to get at others api's.
 *      01-jan-07       napier  Original
 */

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <assert.h>
#include <errno.h>

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <limits.h>
#include <execinfo.h>

#include "htmlglue.h" /* pdls_html_init_control_block() */
#include "pcl5api.h"
#include "psglue.h" /* pdls_ps_init_control_block() */
#include "pdfglue.h" /* pdls_pdf_init_control_block() */
#include "diglue.h" /* pdls_di_init_control_block() */
#include "xlapi.h"
#include "ppdsapi.h"
#include "hexapi.h"
#include "iapi_from_x.h"
#include "pdls.h"
#include "hbpfamily.h"
#include "testconfig.h"
#include "giveback.h"
#include "graphenglue.h"

#include <llib/lmacros.h>
#include <llib/lmemory.h>
#if HAVE_ANRMALLOC
#include <anrmalloc/anrmalloc.h>
#endif
#include <pthread.h>
#include <ctype.h>
#include <blockdevicepage.h> /* block_device_page_set_max_concurrent_clients() */
#include <ximager5.h>
#include "cfs.h"
#include "cfsconfig.h" /* cfs_add_device() */
#include <graphentestbackend.h>
#include "backend.h"

typedef struct {
   char ** filename;
   char ** checksums;
   int n_files;
}FileList;

/*Startinterpreter argument struct*/
typedef struct _InterpreterArgs {
   InterpreterControlBlock interpreter;
   pthread_t pth;
   const char *output_filename;
   const FileList * file_list;
   int thread_id;
   int loop_count;
}InterpreterArgs;


#define __NON_INSTRUMENT_FUNCTION__    __attribute__((__no_instrument_function__))
#define PTRACE_OFF        __NON_INSTRUMENT_FUNCTION__
#define REFERENCE_OFFSET "REFERENCE:"
#define FUNCTION_ENTRY   'E'
#define FUNCTION_EXIT    'L'
#define END_TRACE        "EXIT"

static FILE *fp_trace = NULL;

void
__attribute__ ((constructor))
__NON_INSTRUMENT_FUNCTION__
trace_begin (void)
{
    fprintf(stdout, "----------------- %s: compile with -finstrument-functions called....\n", __func__);
    if (NULL == fp_trace) {
        fp_trace = fopen("trace.out", "w");
    }
}


void
__attribute__ ((destructor))
__NON_INSTRUMENT_FUNCTION__
trace_end (void)
{
    fprintf(stdout, "----------------- %s: called....\n", __func__);
    if(fp_trace != NULL) {
        fprintf(fp_trace, END_TRACE " %ld\n", (long)getpid());
        fclose(fp_trace);
     }
}

static int __stepper=0;

void
__NON_INSTRUMENT_FUNCTION__
__cyg_profile_func_enter(void *this_fn, void *call_site) {
    L_UNUSED_VAR(call_site);
    if (NULL != fp_trace) {
//      int i=0;
//      for( ; i<__stepper; i++ ) fprintf(fp_trace, " ");
//      char func_addr[8];
//      char * func_name = NULL;
//      sprintf(func_addr, "0%x", (unsigned int)this_fn);

      //func_name = find_func_mapping(func_addr);
      fprintf(fp_trace, "%c %p\n", FUNCTION_ENTRY, this_fn);
    }

//  print_stack_trace(fp_trace);
  __stepper ++;
} /* __cyg_profile_func_enter */

void
__NON_INSTRUMENT_FUNCTION__
__cyg_profile_func_exit(void *this_fn, void *call_site) {
    L_UNUSED_VAR(this_fn);
    L_UNUSED_VAR(call_site);

//  int i=0;
  __stepper --;
//  for( ; i<__stepper; i++ ) printf(" ");
//  if(fp_trace != NULL) fprintf(fp_trace, "L:  %p %p\n", this_fn, call_site);
} /* __cyg_profile_func_enter */

/*
 * If access() would check S_ISDIR(), we wouldn't need this.
 */
static int
check_file (const char * filename)
{
   struct stat statbuf;
   int errnum = 0;
   int fd = open (filename, 0);
   if (fd < 0) {
      errnum = errno;

   } else if (0 != fstat (fd, &statbuf)) {
      errnum = errno;

   } else if (S_ISDIR (statbuf.st_mode)) {
      errnum = EISDIR;
   }

   if( fd >= 0 )
       close (fd);

   return errnum;
}

static const struct {
   char * name; /* The pdl's proper name, for help */
   char * arg; /* The thing you pass to -e to get this Emulator */
   char * pjlname; /* @PJL ENTER LANGUAGE=<name> <-- that name; "|"-separated
                    * list of acceptable values. */
   int (*init) (InterpreterControlBlock *); /* Its setup function */
} pdls[] = {
   { "PCL5",        "pcl", "PCL|PCL5",              PCL5_init_control_block },
   { "XL/PCL6",     "xl",  "PCLXL",                 XL_init_control_block },
   { "PS",          "ps",  "PS|Postscript",         pdls_ps_init_control_block },
   { "Hex",         "hex", "HEX",                   HEX_init_control_block },
   { "PDF",         "pdf", "PDF|PDFApp",            pdls_pdf_init_control_block },
   { "DirectImage", "di",  "JPG|PNG|TIFF|GIF|BMP",  pdls_di_init_control_block },
   { "PPDS",        "ppds", "PPDS",                 PPDS_init_control_block },
   { "HTML",        "html", "HTML|HTMLApp",         pdls_html_init_control_block },
   { NULL, NULL, NULL, NULL }
};

static bool
match_pjlname (const char * pjlname,
               const char * teststr)
{
   const char * s;
   int testlen = strlen (teststr);

   /* Can't use strtok(), because that modifies the string. */
   while (NULL != (s = strchr (pjlname, '|'))) {
      if ((testlen == s - pjlname) &&
          (0 == strncasecmp (teststr, pjlname, testlen)))
         return true;
      pjlname = s + 1; /* +1 to skip the '|' */
   }

   return 0 == strcasecmp (teststr, pjlname);
}

bool
pdls_look_up_interpreter (const char * s,
                          InterpreterControlBlock * icb)
{
   unsigned i;

   for (i = 0 ; pdls[i].name != NULL ; i++)
      if (0 == strcasecmp (pdls[i].arg, s) ||
          match_pjlname (pdls[i].pjlname, s)) {
         if (icb)
            pdls[i].init (icb);
         return true;
      }
   printf ("FATAL: Unknown interpreter '%s'\n", s);
   abort ();
}

static const struct {
   struct option option;
   const char * arg_description;
   const char * help_string;
} progopts[] = {
/*
   {
      { "long-form option", number of arguments required, ????, 'short-form option' },
      data type or argument,
      "Description of option."
   },
*/
   {
      { "help", 0, NULL, 'h' },
      NULL,
      "Print out a summary of command-line options."
   },
   {
      { "emulator", 1, NULL, 'e' },
      "emulator",
      "Which Emulator to run."
   },
   {
      { "simple", 0, NULL, 's' },
      NULL,
      "Print page checksums to stdout instead of creating PAM files."
   },
   {
      { "testmode", 1, NULL, 't' },
      "config file",
      "Enable \"test\" mode; option is the path to the test config file."
   },
   {
      { "memsize", required_argument, NULL, 'm' },
      "# bytes",
      "Desired heap size, in bytes."
   },
   {
      { "device", 1, NULL, 'd' },
      "device name",
      "Name of the graphen testing backend device to use."
   },
   {
      { "checksum", 1, NULL, 'c' },
      "string",
      "Comma-separated list of expected page checksums.\n"
      "\tDon't write PAM files for pages whose checksums match.\n"
      "\tDO NOT use this when giving multiple job files."
   },
   {
      { "output-format", 1, NULL, 'O' },
      "string",
      "Control how to \"print\" output pages."
   },
   {
      { "output-filename", 1, NULL, 'o' },
      "path",
      "Prefix for generated pam file names.  Defaults to \"PSPAGE\"."
   },
   {
      { "hbp-family", 1, NULL, 'H' },
      "number",
      "The supported host-based printing family.  Defaults to \"0\" (invalid)."
   },
   {
      { "tracecalls", 0, NULL, 'T' },
      NULL,
      "Trace calls to XImager by setting the TRACECALLS pragmatic."
   },
   {
      { "thread", 1, NULL, 'r' },
      "number",
      "Randomly run all jobs given in all threads. ONLY FOR XL JOBS!!!. Defaults to \"1\". "
   },
   {
      { "no-random", 0, NULL, 'N' },
      NULL,
      "Do not randomize jobs. Used with --thread option."
   },
   {
      { "loop-count", 1, NULL, 'l' },
      "number",
      "Number of times to loop thru the file-list. Defaults to \"1\". "
   },
   {
      { "allocator", required_argument, NULL, 'a' },
      "glibc|custom",
      "Use the specified allocator.  Defaults to custom"
   },
   {
      {"mini-version", no_argument, NULL, 'v'},
      NULL,
      "Force Xi small memory mode"
   },
   {
      {"resolution", 1, NULL, 'R'},
      NULL,
      "Set default resolution in dots per inch. This can be overriden by the job."
   },
   {
      {"nup", 1, NULL, 'P'},
      NULL,
      "Sets the default number of pages per side. This can be overriden by the job."
   },
   {
      { "xi-threads", 1, NULL, 'x' },
      "number",
      "Number of threads used to process XI display list in parallel. Defaults to \"1\". "
   },
   {
      {"warnings-fatal", 0, NULL, 'W'},
      NULL,
      "Set run-time warnings fatal",
   },
   {
      {"cfstab-filename", 1, NULL, 'C'},
      "path",
      "Override the CFS device mountings"
   },
   {
      {"barcode-enable", 0, NULL, 'b'},
      NULL,
      "Override barcode enable flag"
   },
   {
      { NULL, 0, NULL, 0 },
      NULL,
      NULL
   }
};

static bool randomize = false;

char *
make_getopt_optstring (void)
{
   int i;
   char * optstring = malloc (2 * sizeof (progopts) / sizeof (progopts[0]) + 1);
   char * p = optstring;
   assert (optstring);

   /* man 3 getopt, search for "struct option" */
   for (i = 0 ; progopts[i].option.name != NULL ; i++) {
      *p++ = progopts[i].option.val;
      if (progopts[i].option.has_arg)
         *p++ = ':';
      if (2 == progopts[i].option.has_arg)
         *p++ = ':'; /* two colons for optional args */
   }
   *p = '\0';

   return optstring;
}

struct option *
make_getopt_options (void)
{
   unsigned i;
   unsigned n_options = sizeof (progopts) / sizeof (progopts[0]);
   struct option * options = malloc ((n_options + 1) * sizeof (struct option));
   assert (options);

   /* This will copy the null-terminator, too. */
   for (i = 0 ; i < n_options ; i++)
      options[i] = progopts[i].option;

   return options;
}

/*
 * Extract the checksum string from the argument.
 *      input_argv - will be in format prnJob.prn:chksumval1,chksumval2
 *      filename - after extarctiion, this output parameter will contain the filename only without the
 *              checksums after the filename
 */
static char *
extract_checksum_filename(const char * input_argv, char **filename)
{
    char *checksum = NULL, *p;

    *filename = strdup(input_argv);

    p = strstr(*filename, ":");

    if( p != NULL )
    {
        checksum = p + 1;
        *p = '\0';
    }

    return checksum;
}

static void
help (const char * program_name,
      int exit_status)
{
   unsigned i;

   printf ("Interpret Page Description Language data, usually into image files.\n");
   printf ("Input data comes from standard input.\n");

   printf (" Usage: \n");
   printf ("   Normal:          %s [options] < filePath/testcase\n", program_name);
   printf ("   Multi-thread:    %s [options] filePath/testcase1[:checksum1] filepath2/testcase2[:chksum2a, chksum2b]\n", program_name);
   printf ("\n");
   printf (" Options:\n");
   for (i = 0 ; progopts[i].help_string != NULL ; i++) {
      printf ("    -%c, --%s%s%s\n\t%s\n",
              progopts[i].option.val,
              progopts[i].option.name,
              progopts[i].arg_description ? "=" : "",
              progopts[i].arg_description ? progopts[i].arg_description : "",
              progopts[i].help_string);
   }
   printf ("\n");
   printf (" supported pdls:\n");
   for (i = 0 ; pdls[i].name != NULL ; i++)
      printf ("    %-4s for %s\n", pdls[i].arg, pdls[i].name);
   printf ("\n");
   printf (" supported output formats:\n");
   pdls_pretty_print_output_formats (stdout);
   printf ("\n");
   printf (" supported graphen devices:\n");
   graphen_test_backend_describe_device_names (stdout);
   printf ("\n");

   exit (exit_status);
}

static int
randomize_indeces(int indexArray[], int numIndeces, int seed) {
   double rnd_factor;
   int i, j;
   bool valueUsed = 0;

   srand((unsigned int)seed);
   for (i = 0; i < numIndeces; i++) {
      /* assign random index */
      do {
         rnd_factor = (double)rand()/(double)RAND_MAX;
         indexArray[i] = rnd_factor * numIndeces;

         /* check if index has already been randomized
          * do this so we don't repeat the values
          */
         valueUsed = (i != 0);
         for (j = i - 1; j >= 0; j--) {
            valueUsed = indexArray[j] == indexArray[i];
            if (valueUsed) break;
         }
      } while(valueUsed);
   }

   return 0;
}

void *
run_all_jobs (void *args)
{
   InterpreterArgs *interp_args = (InterpreterArgs *)args;
   int i;
   int j;
   int rc = 0;

   for (j = 0; j < interp_args->loop_count; j++) {
      /* randomize here */
      int indexArr[interp_args->file_list->n_files];

      if (randomize) {
         srand((unsigned int)interp_args->thread_id);
         randomize_indeces(indexArr, interp_args->file_list->n_files, rand() + interp_args->thread_id);
      }
      else {
         for( i = 0; i < interp_args->file_list->n_files; i++)
            indexArr[i] = i;
      }

      /* now run the randomized jobs */
      for (i = 0; i < interp_args->file_list->n_files; i++) {
/*
          printf("Processing %s:%s -> %s\n",
             interp_args->file_list->filename[indexArr[i]],
             interp_args->file_list->checksums[indexArr[i]],
             interp_args->output_filename);
*/
         rc = startInterpreter (&interp_args->interpreter,
                                interp_args->output_filename,
                                interp_args->file_list->checksums[indexArr[i]],
                                interp_args->file_list->filename[indexArr[i]]);
         if (rc !=0)
            break;
      }
   }

   return L_INT_TO_PTR (rc);
}

FileList *
gather_all_files(int input_argc, const char **input_argv, int size) {
   FileList * list = NULL;
   char **files = NULL;
   char **checksums = NULL;
   int i;
   int j;

   files = (char **) calloc(size, sizeof(char *));
   if(files == NULL) {
      printf("Failed to allocate filename space in gather_all_files\n");
      return NULL;
   }
   checksums = (char **) calloc(size, sizeof(char *));
   if(checksums == NULL) {
      printf("Failed to allocate checksum space in gather_all_files\n");

      free(files);

      return NULL;
   }
   list = (FileList *) calloc(1,sizeof(FileList));
   if(list == NULL) {
      printf("Failed to allocate FileList space in gather_all_files\n");

      free(files);
      free(checksums);

      return NULL;
   }

   for (i = optind, j = 0; i < input_argc; i++, j++)
      checksums[j] = extract_checksum_filename(input_argv[i], &files[j]);

   list->filename = files;
   list->checksums = checksums;
   list->n_files = size;
   return list;
}

#if HAVE_ANRMALLOC
static void
_abort_handler (const char * file,
                    int line,
                    const char * message)
{
    printf ("%s:%d: %s", file, line,message);
    abort();
}
#endif

extern void ufstinit(void); /*TODO these should be dead.... */
extern void ufstexit(void);

/*
 * Perform any actions required to generally initialize the pdls
 * subsystem.
 */

static int
initialize_imager(int argc, char **argv)
{
   if (getenv("CFS_TAB_FILE"))
      cfs_prologue();
   else
      _cfs_prologue_no_cfstab();

   graphen_test_backend_init();
   xiInitImager(argc, argv);
   ufstinit();

   /* postscript and html expect %systmp% to exist, and expect it to be
    * ./tmp/cfs on sim.   Do *NOT* allow reveal on this device; this device
    * is used to point to the direct job file which may be on removable
    * storage, and reveal'd files will cause a SIGBUS if the user removes
    * the storage device.
    * Create the directory in case that has not been done yet.
    * The user's umask will probably make the permission stricter, but using 0777
    * here allows the choice of making it wide open.
    * pdls should not dictate the choice.
    *
    * Use a system call to create parent directory if needed. */
   (void)system("mkdir -m 0777 -p ./tmp/cfs");
   cfs_add_device ("%systmp%", "./tmp/cfs", CFS_DEVICE_WRITE | CFS_DEVICE_READ);
   cfs_mount ("%systmp%");
   return (0);
}
void
shutdown_imager (void)
{
   ufstexit ();
   TermImager ();

   /*
    * At this point we should have no live malloc'd devices.
    * If we were running single-threaded, iapi_from_stdin() already
    * deleted the device he created, and DEFAULTDEVICE should return NULL
    * for this thread.
    * If we were running multithreaded, all new devices were created on
    * background threads, and DEFAULTDEVICE fort his thread still holds
    * the pointer to const mem installed by graphen_test_backend_init() (boo!),
    * and attempting to free that here will cause a crash.
    */
   graphen_test_backend_term (NULL);
   cfs_exit (0); /* CFS_EXIT_NORMAL */
}

#if HAVE_ANRMALLOC
static void
shutdown_anrmalloc(void)
{
   anr_malloc_reclaim();
   anr_malloc_teardown();
}
#endif

typedef void (*AllocatorCleanupFunction)(void);

int
main (int argc,
      char * argv[])

/*
 * Entry point for the standalone pdl driver.
 *
 * This basically just parses the command line and calls the appropriate
 * functions to do the real work.
 */

{
   char * program_name = argv[0];
   int memsize;
   int c;
   char * optstring;
   struct option * options;
   const char * checksum_string = NULL;
   const char * output_filename = "PSPAGE";
   InterpreterControlBlock interpreter;
   int rc;
   int i = 0;
   int concurrency_on =0;
   int n_threads = 1;
   int n_files = 0;
   int n_loop = 1;
#if HAVE_ANRMALLOC
   bool use_custom_allocator = true;
#else
   bool use_custom_allocator = false;
#endif
   AllocatorCleanupFunction allocator_shutown = NULL;
   int mini_version = 0;
   int resolution = -1;
   int n_up = -1;
   bool warnings_fatal = false;

   /* since a primary mode for running this app is with output redirected
    * to a log file, make sure that everything winds up in the log file
    * in the event of an untimely and uncontrolled demise. */
   setlinebuf (stdout);

   memset (&interpreter, 0, sizeof (interpreter));

   memsize = 128 * 1024 * 1024;

   optstring = make_getopt_optstring ();
   options = make_getopt_options ();

   while (-1 != (c = getopt_long (argc, argv, optstring, options, NULL))) {
      switch (c) {
      case 'h':
         help (program_name, EXIT_SUCCESS);
         break;

      case 'e':
      {
         char * name = optarg;

         if (name && *name == '=')
            name++;

         if (! name)
         {
            fprintf (stderr, "%s: option -e requires an argument!\n",
                     program_name);
            exit (1);
         }

         if (! pdls_look_up_interpreter (name, &interpreter))
         {
            int i;
            fprintf (stderr,
                     "%s: Unknown PDL name \"%s\".  Please choose one of\n",
                    program_name, name);
            for (i = 0 ; pdls[i].name != NULL ; i++)
               fprintf (stderr, "   %-4s for %s\n", pdls[i].arg, pdls[i].name);

            exit (1);
         }
      }
      break;

      case 's':
         /*
          * Want to do the "simple" version of tests (e.g., during the build
          * so don't generate PAM files but rather, generate checksums.
          */
         pdls_set_output_form( 'c' );
         break;

      case 't':
         /*  set the unit test mode flag ..
          *  This will cause more debug information to dump
          *  to standard out such as Tray ,Size,Type selections so
          *  the unit tests can verify correct behavior.
          */
         if (optarg && *optarg == '=')
            optarg++;
         setenv ("PDLS_UNIT_TEST_MODE", optarg, 1);
         break;

      case 'm':

         {
            char c;
            uint32_t multiplier = 0;

            if(2 == sscanf(optarg, "%d%c", &memsize, &c))
            {
               if('m' == tolower(c))
                  multiplier = 1024 * 1024;
               else if('k' == tolower(c))
                  multiplier = 1024;
               else{
                  fprintf(stderr, "%s: Invalid size multiplier %c."
                          "  Use either K (kilobytes) or M (megabytes)\n",
                          argv[0], c);
                  exit(EXIT_FAILURE);
               }

            }else if(0 != (memsize = atoi(optarg)))
               multiplier = 1;

            memsize *= multiplier;
//            totalmem = 4294967296;

            if(!memsize){
//                memsize = INT_MAX;
               fprintf(stderr,
                       "%s: Invalid heap size \"%s\"\n", argv[0], optarg);
               fprintf(stderr, "Format is number of bytes, or"
                       " a number folled immediately bo one of"
                       " the following multipliers:\n");
               fprintf(stderr, "    M     (mega) 1024 * 1024\n");
               fprintf(stderr, "    K     (kilo) 1024\n");

//               fprintf(stderr, "    Using Memory: %u\n", memsize/multiplier);
               exit(EXIT_FAILURE);

            }



         }
         break;

      case 'd':
         /* Which graphen device to use */
         setenv ("DEVICE", optarg, 1);
         break;

      case 'c':
         checksum_string = optarg;
         break;

      case 'o':
         output_filename = optarg;
         break;

      case 'H':
         {
            char * hbp_family = optarg;

            if (hbp_family && *hbp_family == '=')
               hbp_family++;

            if (! hbp_family) {
               fprintf (stderr, "%s: option -H requires an argument!\n",
                        program_name);
               exit (1);
            }

            pdls_set_hbp_family (atoi (hbp_family));
            break;
         }

      case 'O':
         /* Output format. */
         pdls_set_output_form_argument (program_name, optarg);
         break;

      case 'T':
         xiPragmatics ("TRACECALLS", 1);
         break;

      case 'r':
         {
            char * num_threads = optarg;

            if (num_threads && *num_threads == '=')
               num_threads++;

            if (! num_threads) {
               fprintf (stderr, "%s: option -r requires an argument!\n",
                        program_name);
               exit (1);
            }
            printf("Will ONLY enable Concurrency Mode if testcase(s) is from file and it's a XL job\n");
            concurrency_on = 1;
            randomize = true;
            n_threads = atoi(num_threads);

#ifdef BLOCK_DEVICE_PAGE_SET_MAX_CONCURRENT_CLIENTS
            block_device_page_set_max_concurrent_clients(n_threads + 1);
#endif
         }
         break;

      case 'N':
         randomize = false;
         break;

      case 'a': /* choose an allocator */
         {
            if(0 == strcmp("glibc", optarg))
               use_custom_allocator = false;
            break;
         }
      case 'l':
         if (! optarg) {
            fprintf (stderr, "%s: option -l requires an argument!\n",
                     program_name);
            exit (1);
         }
         n_loop = atoi(optarg);
         if(n_loop < 0)
            n_loop = 1;
         break;

      case 'v':
         mini_version = 1;
         break;
      case 'R' :
         resolution = atoi(optarg);
         break;

      case 'P':
         n_up = atoi(optarg);
         break;

      case 'x':
         if ((!optarg) || (atoi(optarg) <= 0)) {
            fprintf (stderr, "%s: option -x requires a positive integer argument!\n",
                     program_name);
            exit (1);
         }
         break;

      case 'W':
         warnings_fatal = true;
         break;

      case 'C':
         setenv ("CFS_TAB_FILE", optarg, 1);
         break;

      case 'b':
         /* enable barcode */
         /* TODO: install barcode symbols here */
         break;

      case '?':
         help (program_name, EXIT_FAILURE);

      }
   }

   free (optstring);
   free (options);

   /*
    * Scan the remaining arguments to see if we have filenames.
    * If we have more than one filename, and one of them is stdin,
    * then we need to throw an error.  But if there's only one, and
    * it is "-", then the user wants stdin.
    */
   if (((argc - optind) == 1) && (0 == strcmp (argv[optind], "-"))) {
      printf ("/* only one arg, and it's -.  Pretend we had none. */\n");
      /* only one arg, and it's "-".  Pretend we had none. */
      --argc;

   } else {

      for (i = optind ; i < argc ; i++) {
         char *filename = NULL;
         char *checksum_string_from_filename;

         if (0 == strcmp (argv[i], "-")) {
            fprintf (stderr, "%s: Can't mix stdin (-) and normal filenames\n",
                     argv[0]);
            exit (EXIT_FAILURE);
         }
         /*
          * Check if we have checksum values attached after the filename.
          * This is attached after a colon(':') without white spaces.  If we
          * find a extract the checksum and store it somewhere.  This process
          * is useful when multiple files will be passed for checksum testing
          * during unit tests.
          * This is just a *dummy* extraction.  We actually just want to get the
          * filename without the trailing checksum for check_file().
          */
         checksum_string_from_filename =
             extract_checksum_filename (argv[i], &filename);

         if (checksum_string && checksum_string_from_filename) {
             fprintf (stderr,
                      "%s: Can't mix filename:checksums and --checksum\n",
                      argv[0]);

             free(filename);

             exit (EXIT_FAILURE);
         }

         /*
          * Check the input file for validity now, before we do lots of setup.
          * Bailing out on an invalid command-line parameter in the middle of
          * the job, with the error message buried in tons of debugging output,
          * does not meet the "user friendly" standard.  Yes, there is a race
          * condition.  No, we don't really care.
          */
         rc = check_file (filename);
         if (0 != rc) {
            fprintf (stderr, "%s: %s: %s\n",
                     argv[0], argv[i], strerror (rc));
            exit (EXIT_FAILURE);
         }
         else
            n_files++;

         free (filename);
      }
   }

   /*
    * Arguments look okay, initialize.
    */

   if (interpreter.emulName[0])
      printf ("Using command-line default interpreter %s\n",
              interpreter.emulName);
   else
      printf ("No default interpreter specified, will look in datastream\n");

   printf( "Current heap size is %d\n", memsize );


   if(use_custom_allocator)
   {
#if HAVE_ANRMALLOC
         LMemoryVTable vtable = {
            anr_malloc_with_return,
            anr_calloc_with_return,
            anr_realloc_with_return,
            anr_free_with_return,
            anr_malloc_mark,
            anr_malloc_if_possible_with_return,
            anr_malloc_if_available_with_return,
            anr_malloc_total_bytes,
            anr_malloc_free_bytes,
            anr_malloc_get_error,
            anr_malloc_set_error
         };

         l_set_memory_vtable (&vtable);
         printf("Using anrmalloc\n");
         anr_malloc_init(memsize, memsize <<1,
                         (MoreMemoryFunction)give_back_anr, NULL,
                         (MemoryFullFunction)memory_full,
                         (AbortFunction)_abort_handler );

         allocator_shutown = shutdown_anrmalloc;
#else
         printf ("No custom allocator available\n");
         abort ();
#endif
   } else {
      printf("Using glibc (unlimited heap)\n");
   }

   if(resolution != -1)
      backend_set_default_resolution(resolution);

   if(n_up != -1)
      backend_set_default_n_up(n_up);

   l_set_warnings_fatal(warnings_fatal);

   xiPragmatics ("SMALLMEMORYMODEL", mini_version);

   initialize_imager(argc, argv);

   xiPragmatics ("MINIVERSION", mini_version);

   /*
    * Run.
    */

   if (optind == argc) {
      rc = startInterpreter (&interpreter, output_filename, checksum_string,
                             NULL);

   } else {
      FileList *file_list = gather_all_files(argc,(const char **)argv, n_files);
      InterpreterArgs *interp_args = calloc(n_threads,sizeof(InterpreterArgs));

      if (checksum_string != NULL) {
         if (n_files > 1 || file_list[0].checksums[0] != NULL) {
            fprintf (stderr,
                     "%s: Can't use --checksum with multiple files\n"
                     "    Use the form \"file1:checksums1\" instead\n",
                      argv[0]);
            exit (EXIT_FAILURE);
         }
         /* Sneaky.  This works because we free the filenames, not the
          * checksums. */
         file_list[0].checksums[0] = (char*) checksum_string;
      }

      if(concurrency_on && strcasecmp(interpreter.emulName, "PCLXL") == 0) {
         for (i = 0; i < n_threads; i++) {
            char buffer[30];
            /*Assign a unique name for each thread*/
            snprintf(buffer,sizeof(buffer), "THREAD%02d_PAGE", i+1);
            interp_args[i].output_filename = strdup(buffer);
            interp_args[i].interpreter = interpreter;
            interp_args[i].file_list = file_list;
            interp_args[i].thread_id = i;
            interp_args[i].loop_count = n_loop;
            rc = pthread_create(&interp_args[i].pth,NULL, run_all_jobs,
                                &interp_args[i]);
         }
         for (i = 0 ; i < n_threads ; i++) {
            pthread_join(interp_args[i].pth,(void *)&rc);
            if (rc != 0)
               printf ("Failed to join thread %d (file %s): %s\n",
                       i, interp_args[i].interpreter.emulName, strerror (rc));
         }
      }
      else {
         if(concurrency_on)
            printf("Only XL jobs are allowed for multi-threaded mode!!! Ignoring request for multi-threaded mode\n");
         interp_args->interpreter = interpreter;
         interp_args->file_list = file_list;
         interp_args->output_filename = output_filename,
         interp_args->thread_id = 0;
         interp_args->loop_count = 1;

         rc = L_PTR_TO_INT (run_all_jobs (interp_args));
         if(rc) {
           printf("Error processing job\n");
         }

      }
      for(i = 0; i < n_files; i++) {
         free(file_list->filename[i]);
      }
      free(file_list->checksums);
      free(file_list->filename);
      free(file_list);
      free(interp_args);
   }

   if (rc != 0)
      printf ("\n\n*****************************************************\n"
              "%s: Interpreter returned failure for %s\n",
              argv[0], optind == argc ? "(stdin)" : argv[i]);

   /*
    * Clean up.
    */

   shutdown_imager ();

   if(allocator_shutown)
      allocator_shutown();

   return rc == 0 ? EXIT_SUCCESS : EXIT_FAILURE;
}



