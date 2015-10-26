#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <assert.h>
#include <unistd.h>
#include <errno.h>

typedef struct
{
    const char * interp_name;
    const char * sig;
    const int  sig_len;
} SniffInfo;


/*
    Our table of file types and signatures.
*/    
static const SniffInfo
    sniff_list[] =
    {
        { "PS", "%!PS", 3 },
        { "PDF", "%PDF", 4 },
        { "JPG", "\xff\xd8", 2 },
        { "GIF", "GIF87a", 6 },
        { "GIF", "GIF89a", 6 },
        { "BMP", "BM", 2 },
        { "TIFF", "MM\x00*", 4 },
        { "TIFF", "II*\x00", 4 },
        { "PNG", "\x89\x50\x4E\x47\x0D\x0A\x01A\x0A", 8},
        { "DCX", "\xB1\x68\xDE\x3A", 4},
        { "MDOC", "\x50\x4B\x03\x04\x14\x00\x06\x00", 8},
        { "MDOC", "\x50\x4B\x03\x04\x14\x00\x08\x08", 8},
        { "MDOC", "\xD0\xCF\x11\xE0\xA1\xB1\x1A\xE1", 8},
        { "XPS", "PK\x03\x04", 4},
        { "XPS", "PK\x03\x05", 4},
        { "PCX", "\x0A\x05\x01\x01", 4},
        { "PCX", "\x0A\x04\x01\x01", 4},
        { "PCX", "\x0A\x03\x01\x01", 4},
        { "PCX", "\x0A\x02\x01\x01", 4},
        { "PCL", "", 0 }
    };

int
main (int argc,
      char * argv[])
{
    FILE *in_file = NULL;
    char * file = NULL;
    unsigned char buffer[16]; /*16 bytes which is enough for isydoc type size */
    int bytes_read, i = 0;
    const char * pdls = "PCL";
    
    if (1 == argc) {
        return EXIT_FAILURE;
    }
    
    do {
        file = argv[1];
        in_file = fopen(file,"r");
        if (!in_file) { /* could not open file */
            fprintf (stderr, "ERROR[%d]: failed to open file[%s]\n", errno, file);
            break;
        }
        
        /* Read at most 16 bytes which is enough for isydoc type size */
        bytes_read = fread(buffer, 1, sizeof(buffer), in_file);
        if (ferror(in_file)) { /* error reading file */
            fprintf (stderr, "ERROR[%d]: error reading file[%s]\n", errno, file);
            break;
        }
        
        while (sniff_list[i].sig_len != 0) {
            if (bytes_read >= sniff_list[i].sig_len && memcmp(buffer, sniff_list[i].sig, sniff_list[i].sig_len) == 0) {
                pdls = sniff_list[i].interp_name;
                break;
            }
            ++i;
        }
    }
    while (0);
    
    if (in_file) fclose(in_file);
    
    printf("%s", pdls );
    return EXIT_SUCCESS;
}