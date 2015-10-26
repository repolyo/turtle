#ifndef __LPROFILE_H__
#define __LPROFILE_H__

#define __NON_INSTRUMENT_FUNCTION__    __attribute__((__no_instrument_function__))
#define PTRACE_OFF        __NON_INSTRUMENT_FUNCTION__
#define REFERENCE_OFFSET "REFERENCE:"
#define FUNCTION_ENTRY   'E'
#define FUNCTION_EXIT    'L'
#define END_TRACE        "EXIT"


#define L_UNUSED_VAR(var) (void)(var)

#endif // __LPROFILE_H__




