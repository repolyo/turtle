#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <assert.h>
#include <unistd.h>
#include <errno.h>
#include <getopt.h>
#include <dirent.h>
#include <errno.h>
#include <stdbool.h>

#define ONE_KILO_BYTES (1024)

#ifndef L_MIN
    #define L_MIN(X,Y) ((X) < (Y) ? (X) : (Y))
#endif

#ifndef L_MAX
    #define L_MAX(a, b)   ((a) > (b) ? (a) : (b))
#endif

#ifndef L_N_ELEMENTS
    #define L_N_ELEMENTS(ary) ((int)(sizeof (ary) / sizeof ((ary)[0])))
#endif

#define HASH_CAPACITY 20000
#define MAX_RECURSION 10
#define CURDIR folders[dir_index]
#define SIG_LEN_MAX 16

#define OPEN_DIR(folder) \
    folders[dir_index] = opendir(folder); \
    if (NULL != folders[dir_index]) dir_index++;

#define CLOSE_DIR if (NULL != folders[dir_index]) { \
    closedir(folders[dir_index]);                   \
    folders[dir_index--] = NULL;                    \
}

/* Flag set by ‘--verbose’. */
static char pdl_type[20] = {0};
static int verbose_flag = 0;
static int recurse_flag;
static short ON = 1;
static short OFF = 0;
/* getopt_long stores the option index here. */
int option_index = 0;
int dir_index = 0;
DIR *folders[MAX_RECURSION];

typedef struct __SniffInfo SniffInfo;

typedef void (*ValueParser) (const char * buffer,
        int length,
        char** language);

typedef struct
{
    const char * key;
    const unsigned short  len;
} Keyword;

struct __SniffInfo
{
    const char * interp_name;
    const char * sig;
    const char * ext;
    const int  sig_len;
    Keyword * markers;
    ValueParser parser;
};

typedef enum
{
    IAPI_SNIFF_NOT_MINE,
    IAPI_SNIFF_POSSIBLY_MINE,
    IAPI_SNIFF_MINE

} IAPI_SniffConfidence;

// Special case: Saved as XPS from Microsoft Office 2007
// Apparently these XPS files inherits the orignal MS doc signature.
// To separate them from regular XPS we look for the existence of
// following keywords: 'word/', 'ppt/', 'xl/'
static const Keyword ms_keys[] ={{"word/document.xml", 17}, {"ppt/presentation.xml", 20}, {"xl/workbook.xml", 15}, {"xl/workbook.bin", 15}, {NULL, 0},};
static const Keyword xl_keys[] ={{"LANGUAGE = PCLXL", 16}, {"LANGUAGE=PCLXL", 14}, {NULL, 0},};
static const Keyword pcl_keys[] ={{"LANGUAGE=PCL", 12}, {NULL, 0},};

static const char pjl_magic[] = "\033%-12345X";
static void mini_pjl_parse_pjl_from_buffer (const char * buffer, int length, char** language);

/*
    Our table of file types and signatures.
*/    
static const SniffInfo
    sniff_list[] =
    {
        { "Office2007", "\x50\x4B\x03\x04\x14\x00\x06\x00", "docx|xlsx|pptx", 8, (Keyword*)&ms_keys},
        { "MDOC", "\x50\x4B\x03\x04\x14\x00\x08\x08", "doc|xls|ppt", 8},
        { "MDOC", "\x50\x4B\x03\x04\x14\x00\x08\x00", "doc|xls|ppt", 8, (Keyword*)&ms_keys},
        { "MDOC", "\xD0\xCF\x11\xE0\xA1\xB1\x1A\xE1", "doc|xls|ppt", 8},
        { "PS", "%!PS", "ps", 4 },
        { "PDF", "%PDF\0", "pdf", 4 },
        { "DI", "\xff\xd8\0", "jpeg", 2 }, // JPG
        { "DI", "GIF87a\0", "gif", 6 }, // GIF
        { "DI", "GIF89a\0", "gif", 6 }, // GIF
        { "DI", "BM\0", "bmp", 2 }, // BMP
        { "DI", "MM\x00*", "tiff", 4 }, //  TIFF
        { "DI", "II*\x00", "tiff", 4 }, // TIFF
        { "DI", "\x89\x50\x4E\x47\x0D\x0A\x01A\x0A", "png", 8}, // PNG
        { "DI", "\xB1\x68\xDE\x3A", "dcx", 4}, // DCX
        { "XL", ") HP-PCL XL;", "xl", 12},
        { "XL", "", "xl", 0, (Keyword*)&xl_keys }, // XL
        { "XPS", "PK\x03\x04", "xps", 4},
        { "XPS", "PK\x03\x05", "xps", 4},
        { "PCX", "\x0A\x05\x01\x01", "pcx", 4},
        { "PCX", "\x0A\x04\x01\x01", "pcx", 4},
        { "PCX", "\x0A\x03\x01\x01", "pcx", 4},
        { "PCX", "\x0A\x02\x01\x01", "pcx", 4},
        { "PPDS", "\x1b\x5b\x4b", "ppds", 3},
        { "HTML", "<html>", "html", 6},
        { "PCL", "\033E\033", "pcl", 3 },  /* Traditional PCL */
//        { "PCL", "", "pcl", 0, (Keyword*)&pcl_keys },
        { "PCL", pjl_magic, "PJL", 9, NULL, mini_pjl_parse_pjl_from_buffer },
    };

static const char* ext_msdoc[] = {
    "word/",
    "ppt/",
    "xl/"
};

static void
help (const char * program_name,
      int exit_status)
{
   printf ("File signature sniffer.\n");
   printf (" Usage: \n");
   printf ("   %s [options] filePath\n", program_name);
   printf ("\n");
   printf (" Options:\n");
   printf ("\n");

   exit (exit_status);
}

static struct option long_options[] =
{
  /* These options set a flag. */
  {"verbose", no_argument,       &verbose_flag, 1},
  {"brief",   no_argument,       &verbose_flag, 0},
  /* These options don’t set a flag.
     We distinguish them by their indices. */
  {"recurse", no_argument,       0, 'r'},
  {"add",     no_argument,       0, 'a'},
  {"append",  no_argument,       0, 'b'},
  {"type",  required_argument, 0, 't'},
  {"file",    required_argument, 0, 'f'},
  {0, 0, 0, 0}
};

typedef enum  {
    HELP,
    TYPE,
    RECURSIVE,
    OTHERS
} OPTIONS_E;

static struct {
   struct option option;
   void * value;
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
      { "help", HELP, NULL, 'h' },
      NULL,
      NULL,
      "Print out a summary of command-line options."
    },
    {
       { "type", TYPE, NULL, 't' },
       NULL,
       "doc",
       "Document type."
    },
    {
      { "r", RECURSIVE, NULL, 'r' },
      &OFF,
      NULL,
      "Traverse recursively on dir target."
    },
    {
      { "o", OTHERS, NULL, 'o' },
      &OFF,
      NULL,
      "Others [optional] param."
    },
    {
       { NULL, 0, NULL, 0 },
       NULL,
       NULL
    }
};

static bool getParamAsBool(OPTIONS_E opt) {
    short *val = (short*)progopts[opt].value;
    return (NULL != val && 1 == *val);
}

static char* getParamAsString(OPTIONS_E opt) {
    return (char*)progopts[opt].value;
}


//#define HASH_CAPACITY 20000
static int hash_count = 0;

#define isysdoc_debug_print(FLAG, ...) printf (#FLAG, __VA_ARGS__);

static SniffInfo* isysdoc_filter_sniff (unsigned char const * buffer, int size);

static void hex_dump(const char* buff, unsigned short len) {
    int y = 0;
    for (y = 0; y < len; y++) {
        printf("%02x ",buff[y]);
    }
    printf("\n");
}

/**
 * Get the length of the string starting at @p start, and ending somewhere
 * before @p end, ignoring any whitespace before @p end.
 */
static int
strlen_no_endspace (const char * start,
                    const char * end)
{
    while (isspace (*end) && end > start)
        end--;
    return end - start + 1;
}

/**
 * Find the offset of a substring in a non-terminated buffer.
 *
 * It's like strstr(), but the haystack is not null-terminated, and we'll
 * detect a partial match at the end of the buffer.
 *
 * @param haystack          Buffer to search.
 * @param haystack_length   Usable length of @p haystack.  May contain nuls,
 *                          but the nuls are not considered terminators.
 * @param needle            Nul-terminated string to seek.
 *
 * @returns The offset of the start of the @p needle in the @p haystack.
 *          If there was no match, returns @p haystack_length (that is,
 *          the match is beyond the end of the buffer).
 */
static int
find_substr_offset (const char * haystack,
                    size_t haystack_length,
                    const char * needle)
{
    const char * p = haystack;
    const char * end = haystack + haystack_length;
    size_t needle_index = 0;

    for (p = haystack ; p < end ; p++) {
        if (*p == needle[needle_index]) {
            needle_index ++;
            if ('\0' == needle[needle_index])
                /* Found a full match. */
                break;

        } else if (*p == needle[0])
            needle_index = 1;

        else
            needle_index = 0;
    }

    if (needle_index > 0) {
        /* Found something. */
        int offset = p - haystack;

        if (offset == (int)haystack_length)
            return haystack_length - needle_index;

        /* If we didn't get a full match, then p was one past the end of the
         * haystack, so we have to subtract one. */
        return offset - (needle_index - 1);
    }

    return haystack_length;
}

/**
 * Specialized string match, ignoring differences in whitespace.
 * This exists basically because we're not using a real regex engine or
 * token parser.
 *
 * @param buffer  The haystack.
 * @param length  of @p buffer.
 * @param search  The string to seek.  I hesitate to call this a pattern,
 *                but that's sorta what it is.
 * @return The first character after the matched @p search in @p buffer,
 *         or NULL if it doesn't match.
 */
static const char *
flexible_match (const char * buffer,
                int length,
                const char * search)
{
    const char * p = buffer;
    const char * e = buffer + length;
    int seeking = 0;

    while (p < e) {
//        printf ("*p = '%c'  e - p = %d    search[%d] = '%c'\n",
//                *p, e - p, seeking, search[seeking]);
        if (search[seeking]) {
            if ((*p != search[seeking])) {
                if (isspace (*p)) {
                    if (seeking > 0 && isspace (search[seeking - 1])) {
//                        printf ("flexible space\n");
                        seeking--;
                    } else if (! isalnum (search[seeking])) {
//                        printf ("space at word boundary\n");
                        seeking--;
                    }
                } else {
//                    printf ("no match\n");
                }
            }

            p++;
            seeking++;

        } else {
            /* found it */
            return p;
        }
    }

    return NULL;
}


/**
 * Parse the left and right sides out of a string like "name = value".
 * Leading and trailing whitespace on name and value are trimmed.  (That is,
 * the '=' doesn't need space.)  Internal space is left intact.
 *
 * @param variable Place to store malloc'd string containing the variable name.
 *                 It this comes back non-nul, the caller must free it.
 * @param value Place to store malloc'd string containing the value text.
 *              It this comes back non-nul, the caller must free it.
 * @returns true if the parse succeded, and false otherwise (e.g. if there
 *          was nothing but whitespace)
 */
static bool
parse_assignment (const char * buffer,
                  int buflen,
                  char ** variable,
                  char ** value)
{
    const char * s = buffer;
    const char * e; //= strchr (buffer, '=');

    *variable = NULL;
    *value = NULL;

    while (isspace (*s) && buflen)
        s++, buflen--;

    if (!buflen || !*s)
        return false;

    e = s;
    while (*e != '=' && (e - s < buflen))
        e++;

    if (e - s == buflen)
        return false;

    *variable = strndup (s, strlen_no_endspace (s, e - 1));

    buflen -= e + 1 - s;
    s = e + 1;

    while (isspace (*s) && buflen)
        s++, buflen--;

    if (buflen && *s)
        *value = strndup (s, strlen_no_endspace (s, s + buflen - 1));

    return true;
}

static void
mini_pjl_parse_pjl_from_buffer (const char * buffer, int length, char** language)
{
    int offset;
    const char * token;

    offset = find_substr_offset (buffer, length, "@PJL ENTER ");
    token = flexible_match ((const char*)&buffer[offset], length, "@PJL ENTER ");
    if ( token ) {
        char * variable, * value;
        if (parse_assignment (token, length - offset, &variable, &value)) {
            if (!strncasecmp("POSTSCRIPT", value, 10)) {
                SniffInfo *sig = NULL;
                offset = find_substr_offset (token, length, "%PDF");
                sig = isysdoc_filter_sniff(&token[offset], length);
                if (NULL != sig) {
                    strcpy(*language, sig->interp_name);
                }
                else {
                    strcpy(*language, "PS");
                }
            }
            free (variable);
            free (value);
        }
    }
}

static int
_l_sniff_debug_print_buffer(const unsigned char* buffer, const int size, char **out)
{
    int i;
    {
        char *s = *out;
        for (i = 0; i < size ; i++, s += 3) {
            snprintf(s, size - (i*3), "%02x ", buffer[i]);
        }
        *s = '\0';
    }
    return 0;
}

static SniffInfo*
isysdoc_filter_sniff (unsigned char const * buffer, int size)
{
    SniffInfo *sig = NULL;
    IAPI_SniffConfidence confidence = IAPI_SNIFF_NOT_MINE;
    size_t i = 0;
    int x = 0;
    size_t max_len = 0;

    if (size < 4) {
       isysdoc_debug_print ("%s - size(%d) is too small!\n", __func__, size);
       return NULL;
    }
    for (i = 0; confidence != IAPI_SNIFF_MINE && i < L_N_ELEMENTS (sniff_list); i++) {
        sig = (SniffInfo*)&sniff_list[i];
        if (size < sig->sig_len) {
            continue;
        }
        max_len = L_MAX(max_len, sig->sig_len);
        if (!memcmp(buffer, sig->sig, sig->sig_len)) {
            max_len = sig->sig_len;
            if (NULL != sig->markers) {
                for (x = 0; confidence != IAPI_SNIFF_MINE && x < size - sig->sig_len; x++) {
                    Keyword *marker = sig->markers;
                    char* tmp = (char*)&buffer[x];
                    for ( ;NULL != marker->key; marker++) {
                        if (! strncmp(tmp, marker->key, marker->len)) {
                            confidence = IAPI_SNIFF_MINE;
                            break;
                        }
                    }
                }
            }
            else {
                confidence = IAPI_SNIFF_MINE;
            }
        }
    }

    {
//        char tmp[24];
//        _l_sniff_debug_print_buffer(buffer, 24, (char**)&((char*)&tmp));
//        printf("%s - buffer[%d]: {%s}\n", __func__, 24, tmp);
    }

    return confidence == IAPI_SNIFF_NOT_MINE ? NULL : sig;
}

static const char * file_sniff_type(char* file) {
    SniffInfo *type = NULL;
    size_t thirty_kb = 32 * ONE_KILO_BYTES;
    unsigned char buffer[thirty_kb]; /* 32KB bytes which is enough for isydoc type size */
    int bytes_read, i = 0, t = 0;
    FILE *in_file = NULL;

    bool extmsdoc = false;
    bool found = false;
    char* doctype = getParamAsString(TYPE);

    do {
        in_file = fopen(file,"r");
        if (!in_file) { /* could not open file */
            fprintf (stderr, "ERROR[%d]: %s [%s]\n", errno, strerror(errno), file);
//            exit (0);
            break;
        }

        /* Read at most 16 bytes which is enough for isydoc type size */
        bytes_read = fread(buffer, 1, thirty_kb, in_file);
        if (ferror(in_file)) { /* error reading file */
            fprintf (stderr, "ERROR[%d]: error reading file[%s]\n", errno, file);
            break;
        }
//        else {
//            FILE *fp = fopen("./doc_file", "w+");
//            if(fp != NULL) {
//                fwrite(buffer, 1, bytes_read, fp);
//                fprintf (stdout, "%s: thirty_kb: %d, bytes_read: %d\n", __func__, thirty_kb, bytes_read);
//                fclose(fp);
//           }
//        }

        type = isysdoc_filter_sniff(buffer, bytes_read);
    }
    while (0);

    if (in_file) fclose(in_file);
    if (NULL != type) {
        char *language = strdup(type->interp_name);
        if (type->parser) type->parser(buffer, thirty_kb, &language);
        snprintf(pdl_type, sizeof(pdl_type),  "%s/%s", language, type->ext);
        free(language);

//        snprintf(pdl_type, sizeof(pdl_type),  "%s/%s", type->interp_name, type->ext);
    }
    return pdl_type;
}

static void list_dir(DIR * directory, char* path) {
    char file[1024];
    struct dirent *dir = NULL;
    char* doctype = getParamAsString(TYPE);
//    printf("%s: %s\n", __func__, path);
    do {
        const char* sniff_type = NULL;
        dir = readdir(directory);
        if (NULL != dir) {
            if (dir->d_name[0] == '.') continue; // bypass hidden items
            sprintf(file, "%s/%s", path, dir->d_name);
            bool xps = 0 < strstr(dir->d_name, ".xps");
            DIR * d = opendir(file);
            if(d != NULL) {
                if (getParamAsBool(RECURSIVE)) list_dir(d, file);
            }
            else {
                sniff_type = file_sniff_type(file);
            }
            printf("%s - %s\n", dir->d_name, file_sniff_type(file));

            //if (NULL != sniff_type && (NULL == doctype || !strcmp(doctype, sniff_type)) ) {
            if (NULL != sniff_type  && (NULL == doctype || NULL != strstr(sniff_type, doctype))) {
                //printf("%s\n", verbose_flag ? xps ? file : dir->d_name : "");
            }
        }
    }
    while (dir != NULL);
    closedir(directory);
}

int
main (int argc,
      char * argv[])
{
    char * program_name = argv[0];
    char * file = NULL;
    int c;
    
    memset(folders, 0x00, sizeof(DIR*) * MAX_RECURSION);
//    memset(hash, 0x00, sizeof(hash));

    while (-1 != (c = getopt_long (argc, argv, "orabt:f:", long_options, &option_index))) {
       switch (c) {
       case 0:
         /* If this option set a flag, do nothing else now. */
         if (long_options[option_index].flag != 0)
           break;
         printf ("option %s", long_options[option_index].name);
         if (optarg)
           printf (" with arg %s", optarg);
         printf ("\n");
         break;
       case 't':
         progopts[TYPE].value = optarg;
         break;
       case 'r':
         progopts[RECURSIVE].value = &ON;
         break;
       case 'o':
         progopts[OTHERS].value = &ON;
         break;
       case 'h':
       case '?':
       default:
          help (program_name, EXIT_SUCCESS);
          break;
       }
    }

    if (argc < optind) {
        return EXIT_FAILURE;
    }
    file = argv[optind];

    /* program spits warnings, errors, and status to stdout.  This messes
     * up our stdout, which is supposed to contain only the sniffed type.
     * Temporarily shunt all stdout to stderr... */
//    FILE * save_stdout = stdout;
//    stdout = stderr;

    DIR * directory = opendir(file);
//    printf("%s: tanch is here...\n", __func__);
    if(directory != NULL) {
        list_dir(directory, file);
        printf("\n");
    }
    else {
        printf("%s", file_sniff_type(file));
    }
    if (verbose_flag) printf("match count: %d\n", hash_count);
//    dump_hash();

//    stdout = save_stdout;

    return EXIT_SUCCESS;
}
