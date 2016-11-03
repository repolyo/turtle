
%{
/* * * * * * * * * * * *
 * * * DEFINITIONS * * * fprintf(yyout,"%s: ", #case); \
 * * * * * * * * * * * */
 
#include <assert.h>
#include <string.h>

#define __debug 0
char **names = NULL;
int  current = 1;

typedef enum {
    PDLSNIFF_ZIP	= 0x01,
    PDLSNIFF_XPS 	= 0x02,
    PDLSNIFF_MSDOC  = 0x04,
    PDLSNIFF_DOC 	= 0x08,
	PDLSNIFF_PPT  	= 0x10,
	PDLSNIFF_XLS  	= 0x20,
	PDLSNIFF_PDF 	= 0x40,
	PDLSNIFF_PS 	= 0x80,
	PDLSNIFF_TIF 	= 0x100,
	PDLSNIFF_JPG 	= 0x200,
	PDLSNIFF_BMP 	= 0x400,
	PDLSNIFF_PNG  	= 0x800,
	PDLSNIFF_GIF 	= 0x1000, 
	PDLSNIFF_DCX 	= 0x2000, 
	PDLSNIFF_PCL 	= 0x4000,
	PDLSNIFF_XL 	= 0x8000,
	PDLSNIFF_PJL	= 0x100000,
	PDLSNIFF_HTML	= 0x200000,
    PDLSNIFF_MAX_SIZE = 0xffffffff,
} PdlSniffType;

typedef int (*PdlSniffTypeHandler)(const char* yytext,
        const int yyleng, FILE* yyout);

void 
dumpFile(const char* file)
{
    FILE *fout;
	FILE *fin;	
	char buf[1024*8];
	int len = 0;
	static int count = 0;
	sprintf(buf, "%s_%d.data\0", file, count++);
	fout = fopen(buf, "wb+");
	fin = fopen(file, "rb");
	do {
		if (len > 0) fwrite(buf, len, 1, fout );
		len = feof(fin) ? 0 : fread(buf, 1, sizeof(buf)-1, fin);
	}
	while ( len );
	
	fclose (fin);
	fclose (fout);
}
		
#define PDLSNIFF_NULLFUNC (PdlSniffTypeHandler)(NULL)

/*redefines flex YY_INPUT */
#undef YY_INPUT
static int pdlsniff_yyinput(char* buffer, const int max_size);
#define YY_INPUT(buf, result, max_size)(result = pdlsniff_yyinput(buf, max_size))

#define YY_CASE_HANDLER(case, text, leng, fileout) \
  BEGIN (case); \
  if (pdlsniff_case_handler(PDLSNIFF_##case, #case, leng, fileout)) \
    ;//yyterminate();

/*remove calls to exit()*/
#undef YY_FATAL_ERROR
#define YY_FATAL_ERROR(msg) {fprintf(stderr, "ERROR: %s", msg); assert(0); }

static PdlSniffType pdlsniff_state = 0;
static int totalBytes = 0;
static FILE *fp = NULL;
static int ctr[YY_NUM_RULES];

static void pdlsniff_setup();
static const char* pdlsniff_getname();
static int pdlsniff_printf(const char* text, const int len, FILE* out);

#define YY_USER_ACTION ++ctr[yy_act];
#define YY_USER_INIT { pdlsniff_setup(); fclose(fp); fp = fopen("chunk.bin", "wb+");}

#undef ECHO
#define ECHO

#define yyterminate() { \
   const char *ext = pdlsniff_getname(); \
   ( NULL != ext && NULL != strstr(names[current], ext) ) ? \
		fprintf(yyout, "%s = %s\n", names[current], ext) : \
		fprintf(yyout, "%s = ERROR(%s)\n", names[current], ext); \
   if(names[current] != NULL){ \
      YY_USER_INIT; \
	  yyin = fopen(names[++current],"rb"); \
	  if(yyin == NULL){ \
		if (names[current]) fprintf(stderr,"cat: unable to open %s\n", names[current]); \
		return YY_NULL; \
	  } \
	  YY_NEW_FILE; \
   } else { \
	 return YY_NULL; \
   } \
}

static struct {
    PdlSniffType key;
    PdlSniffTypeHandler func;
	const char* name;
} pdlsniff_lookup_table[] = {
	{PDLSNIFF_ZIP,   	pdlsniff_printf, ".zip"},
	{PDLSNIFF_XPS,   	pdlsniff_printf, ".xps" },
    {PDLSNIFF_DOC,    	pdlsniff_printf, ".doc" },
	{PDLSNIFF_PPT,    	pdlsniff_printf, ".ppt" },
	{PDLSNIFF_XLS,    	pdlsniff_printf, ".xls" },
	{PDLSNIFF_MSDOC,    pdlsniff_printf, ".msdoc" },
    {PDLSNIFF_PDF,      PDLSNIFF_NULLFUNC, ".pdf"},
	{PDLSNIFF_PS,      	PDLSNIFF_NULLFUNC, ".ps"},
	{PDLSNIFF_TIF,     	PDLSNIFF_NULLFUNC, ".tif"},
	{PDLSNIFF_JPG,      PDLSNIFF_NULLFUNC, ".jpg"},
	{PDLSNIFF_BMP,      pdlsniff_printf, ".bmp"},
	{PDLSNIFF_PNG,      PDLSNIFF_NULLFUNC, ".png"},
	{PDLSNIFF_GIF,      PDLSNIFF_NULLFUNC, ".gif"},
	{PDLSNIFF_PCL,      PDLSNIFF_NULLFUNC, ".pcl"},
	{PDLSNIFF_XL,      	PDLSNIFF_NULLFUNC, ".xl"},
	{PDLSNIFF_PJL,      PDLSNIFF_NULLFUNC, ".pjl"},
	{PDLSNIFF_HTML,     pdlsniff_printf, ".html"},
    {PDLSNIFF_MAX_SIZE, PDLSNIFF_NULLFUNC, NULL}
};

static void
pdlsniff_setup()
{
  int i = -1;
  while( pdlsniff_lookup_table[++i].key < PDLSNIFF_MAX_SIZE) {
    if (!__debug) pdlsniff_lookup_table[i].func = PDLSNIFF_NULLFUNC;
  }
}

static const char*
pdlsniff_getname()
{
  int i, id;
  //fprintf(yyout, "\ttanch@%s: pdlsniff_state: %02x\n ", __func__, pdlsniff_state);
  for ( i = 0, id = pdlsniff_lookup_table[0].key; id != PDLSNIFF_MAX_SIZE; i++) {
    if (pdlsniff_state == pdlsniff_lookup_table[i].key) {
      return pdlsniff_lookup_table[i].name;
    }
  }
  return NULL;
}

static void
dump(const char* text, const int len, FILE* out)
{
  int i;
  for (i = 0; i < len; i++) {
    //fprintf(out, "%02x ", text[i]&0xff);
	fprintf(out, "%c ", text[i]&0xff);
  }
}

static int
pdlsniff_printf(const char* text, const int len, FILE* out)
{
  fprintf(out, "\ttanch@%s: text(%d): ", text, len);
  dump(yytext, yyleng, out);
  fprintf(out, "%s\n");
  return 0;
}

static int 
pdlsniff_yyinput(char* buffer, const int max_size) {
  int ret = -1;
  
  ret = feof(yyin) ? 0 : fread(buffer, 1, max_size, yyin);
  fwrite(buffer, ret, 1, fp );
  
  //fprintf(yyout, "tanch@%s: ret: %d, yyin: %p\n", __func__, ret, yyin);
  totalBytes += ret;
  
  return ret;
}

static int
pdlsniff_case_handler(const PdlSniffType type, const char* yytext, const int yyleng, FILE* fileout)
{
  int i;
  PdlSniffType id = 0;
  PdlSniffTypeHandler func;
  
  pdlsniff_state = type;
  //fprintf(yyout, "tanch@%s: pdlsniff_state: %d \n", __func__, pdlsniff_state);
  for ( id = pdlsniff_lookup_table[0].key; id != PDLSNIFF_MAX_SIZE; i++) {
    id = pdlsniff_lookup_table[i].key; 
    func = pdlsniff_lookup_table[i].func;
    //fprintf(yyout, "tanch@%s: id: %d, func: %p\n", __func__, id, func);
    
    if (type == id) {
      return func ? func(yytext, yyleng, fileout) : 1;
    }
  }
  return 0;
}

%}

%option noyywrap

%s ZIP
%s XPS
%s MSDOC DOC PPT XLS
%s PDF
%s PS
%s PJL PCL XL
%s TIF JPG BMP PNG GIF DCX
%s HTML

digit [0-9]
whitespace [ \t\n\r\f]
ws    [ \t]
nonws [^ \t\n]

zip_sig "PK"\x03
xps_sig [\x04\x05]
word_doc "word/document.xml"
ppt_doc "ppt/presentation.xml"
xls_doc ("xl/workbook."[xml|bin])
pjl_sig "%-12345X@PJL"
pcl_sig (\033E\033)
xl_sig ("xl")

ps_sig (%\!PS?)
pdf_sig (%PDF-{digit}.{digit}?)
png_sig (\x89PNG\x0d\x0a\x1a\x0a)
gif_sig (GIF8[79]a)
jpg_sig (\xff\xd8)
tif_sig (MM\x00\*|II\*\x00)
bmp_sig "BM"
html_sig .*("<html"|"<HTML").*">"

%%

%{
/* * * * * * * * * 
 * * * RULES * * *
 * * * * * * * * */
%}

<<EOF>> {
   char msg[200];
   sprintf(msg, "%s -- didn't found matching signature!\n", names[current]);
   YY_FATAL_ERROR(msg);
   return 1;
}

<MSDOC>.*("Word"|"MSWordDoc"|"Word.Document") {
  YY_CASE_HANDLER(DOC, yytext, yyleng, yyout);
  yyterminate();
}

<MSDOC>.*("Presentation"|"Power") {
  YY_CASE_HANDLER(PPT, yytext, yyleng, yyout);
  yyterminate();
}

<MSDOC>.*("Worksheet"|"Excel"|"xls") {
  YY_CASE_HANDLER(XLS, yytext, yyleng, yyout);
  yyterminate();
}

<ZIP>{xps_sig}.*"FixedDocumentSequence" {
	YY_CASE_HANDLER(XPS, yytext, yyleng, yyout);
	yyterminate();
}

<ZIP>{xps_sig} {
  YY_CASE_HANDLER(XPS, yytext, yyleng, yyout);
}

<ZIP>[ \t\n\r\f]? {
	YY_CASE_HANDLER(ZIP, yytext, yyleng, yyout);
	yyterminate();
}

<XPS>\x14\x00\x08\x08 {
  YY_CASE_HANDLER(MSDOC, yytext, yyleng, yyout);
  yyterminate();
}

<XPS>.*"FixedDocumentSequence" {
	YY_CASE_HANDLER(XPS, yytext, yyleng, yyout);
	yyterminate();
}

<XPS>\x14\x00\x06\x00\x08.*{word_doc} {
  YY_CASE_HANDLER(DOC, yytext, yyleng, yyout);
  yyterminate();
}

<XPS>\x14\x00\x06\x00\x08.*{ppt_doc} {
  YY_CASE_HANDLER(PPT, yytext, yyleng, yyout);
  yyterminate();
}

<XPS>\x14\x00\x06\x00\x08.*{xls_doc} {
  YY_CASE_HANDLER(XLS, yytext, yyleng, yyout);
  yyterminate();
}

^{zip_sig} {
  YY_CASE_HANDLER(ZIP, yytext, yyleng, yyout);
}

^\xd0\xcf\x11\xe0\xa1\xb1\x1a\xe1.*MSWord {
  YY_CASE_HANDLER(DOC, yytext, yyleng, yyout);
  yyterminate();
}

^\xd0\xcf\x11\xe0\xa1\xb1\x1a\xe1.*W?o?r?k?b?o?o?k* {
  YY_CASE_HANDLER(XLS, yytext, yyleng, yyout);
  yyterminate();
}

^\xd0\xcf\x11\xe0\xa1\xb1\x1a\xe1* {
  YY_CASE_HANDLER(MSDOC, yytext, yyleng, yyout);
  yyterminate();
}

^{ps_sig} {
	YY_CASE_HANDLER(PS, yytext, yyleng, yyout);
	yyterminate();
}

^{pdf_sig}  {
	YY_CASE_HANDLER(PDF, yytext, yyleng, yyout);
	yyterminate();
}

^{gif_sig}*  {
	YY_CASE_HANDLER(GIF, yytext, yyleng, yyout);
	yyterminate();
}

{pjl_sig} {
	YY_CASE_HANDLER(PJL, yytext, yyleng, yyout);
	yyterminate();
}

^{pcl_sig}* {
	YY_CASE_HANDLER(PCL, yytext, yyleng, yyout);
	yyterminate();
}

^{xl_sig}* {
	YY_CASE_HANDLER(XL, yytext, yyleng, yyout);
	yyterminate();
}

^{jpg_sig}* {
	YY_CASE_HANDLER(JPG, yytext, yyleng, yyout);
	yyterminate();
}
	
^{tif_sig}* {
	YY_CASE_HANDLER(TIF, yytext, yyleng, yyout);
	yyterminate();
}

^{png_sig}* {
	YY_CASE_HANDLER(PNG, yytext, yyleng, yyout);
	yyterminate();
}

^\xB1\x68\xDE\x3A.* { fprintf(yyout, " DCX image: %s\n", yytext); }
^\x0A[\x02\x03\x04\x05]\x01\x01.*  { fprintf(yyout, " PCX Image: %s\n", yytext); }
^\x1b\x5b\x4b.* { fprintf(yyout, " PPDS: %s\n", yytext); }

{html_sig} {
	YY_CASE_HANDLER(HTML, yytext, yyleng, yyout);
	yyterminate();
}

^{bmp_sig}* {
	YY_CASE_HANDLER(BMP, yytext, yyleng, yyout);
	yyterminate();
}

%%

/* * * * * * * * * * * 
 * * * USER CODE * * *
 * * * * * * * * * * *
 */
int main(int argc, char *argv[]) {
	if(argc < 2){
       fprintf(stderr,"Usage: cat files....\n");
       exit(1);
    }
    names = argv;
	//fprintf(stdout,"Scanning: %s...\n", names[current]);
    yyin = fopen(names[current],"rb");
    if(yyin == NULL){
      fprintf(stderr,"cat: unable to open %s\n",
              names[current]);
      yyterminate();
    }
    yylex();
	fclose (fp);
}
