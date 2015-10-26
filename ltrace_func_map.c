/*
 * ltrace_func_map.c
 *
 *  Created on: Oct 22, 2015
 *      Author: chritan
 */

#include <stdio.h>
#include <stdbool.h>
#include <llib/lhash.h>
#include <llib/lstr.h>
#include <unistd.h>

//typedef enum
//{
//    NOTYPE,
//    FSECTION,
//    FFILE,
//    FOBJECT,
//    FFUNC,
//} LFuncType;
//
//typedef struct _LFuncEntry
//{
//    int Num;
//    char func_addr[10];
//    int size;
//    char type[10];
//    char bind[10];
//    char vis[10];
//    int idx;
//    char func_name[50];
//} FuncEntry;

static LHash * func_map;
static char* func_name;
static bool init = false;

static void del_value (lpointer data) __attribute__((no_instrument_function));
static void
del_value (lpointer data)
{
    l_free(data);
}

static bool l_hash_str_equal_func (lconstpointer key1, lconstpointer key2)
    __attribute__((no_instrument_function));
static bool
l_hash_str_equal_func (lconstpointer key1,
                       lconstpointer key2)
{
    return !strncmp(key1, key2, 8);
}

static char* str_ncreate(const char *cstr) __attribute__((no_instrument_function));
static char *
str_ncreate(const char *cstr)
{
    int len = strlen(cstr);
    char *str = l_malloc(len+1);
    strncpy(str, cstr, len);
    return str;
}

static bool find_func_entry(lpointer key, lpointer value, lpointer user_data) __attribute__((no_instrument_function));
static bool
find_func_entry (lpointer key, lpointer value, lpointer user_data)
{
    char* findK = user_data;
    bool found = l_hash_str_equal_func(key, findK);
    if (found) {
        func_name = value;
    }
    return found;
}

char* find_func_mapping(char* key) __attribute__((no_instrument_function));
char*
find_func_mapping(char* key)
{
    func_name = NULL;
    l_hash_foreach(func_map, find_func_entry, key);
//    else {
//        extern void _l_hash_dump(LHash * hash);
//        _l_hash_dump(func_map);
//    }
    return func_name;
}

int load_func_mapping(const char* symtable) __attribute__((no_instrument_function));
int
load_func_mapping(const char* symtable)
{
    FILE * fmap = NULL;
    LStr* gen_file = l_str_create_printf("./%s.h", symtable);
    char* header = l_str_to_c_str(gen_file);
    if ( access(header, R_OK) != F_OK) {
        printf("tanch@%s: %s, called...\n", __func__, symtable);

        fmap = fopen(symtable, "r");
        if (fmap) {
            int cnt;
            char line [ 128 ]; /* or other suitable maximum line size */
            char buff1[15];
            char buff2[15];
            char buff3[15];
            char buff4[15];
            char buff5[15];
            char buff6[15];
            char func_addr [ 50 ];
            char func_name [ 50 ];

            bool start_entry_found = false;
            func_map = l_hash_new_full (l_hash_int_hash_func,
                    l_hash_str_equal_func, del_value, del_value);

            while ( fgets ( line, sizeof line, fmap ) != NULL ) { /* read a line */
                if ( !start_entry_found ) {
                    start_entry_found = NULL != strstr(line, "Num:    Value  Size Type    Bind   Vis      Ndx Name");
                    continue;
                }
                // Num:    Value  Size Type    Bind   Vis      Ndx Name
                // 2742: 087ade60 19968 OBJECT  LOCAL  DEFAULT   27 mapped_files
                cnt = sscanf(line, "%s %s %s %s %s %s %s %s",
                        buff1, func_addr, buff2, buff3, buff4, buff5, buff6, func_name);
                if ( cnt ) {
                    char *key = str_ncreate(func_addr);
                    if (strcmp("00000000", key)) {
                        char *value = str_ncreate(func_name);
                        if (l_hash_str_equal_func(key, "082fb408")) {
                            printf("tanch@%s: found 082fb408 = %s\n", __func__, value);
                        }
                        l_hash_insert(func_map, key, value);
                    }
                }
                else {
                    start_entry_found = false;
                }
            }

            char *val = find_func_mapping("082fb408");
            printf("found 082fb408 = %s\n", val);
            fclose(fmap);
            init = true;
        }
        else {
            perror ( symtable ); /* why didn't the file open? */
        }

    }
    l_str_force_destroy (gen_file);
    return 1;
}

void unload_func_mapping() __attribute__((no_instrument_function));
void
unload_func_mapping()
{
    l_hash_destroy(func_map);
}
