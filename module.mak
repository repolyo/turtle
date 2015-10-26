#; -*-Makefile-*-
d := .

LIBUFST_LIBS ?= -L$(LIBUFST_LIB_DIR) -lufst
LIBITYPE_LIBS ?= -L$(LIBITYPE_LIB_DIR) -litype -lityputil

app_OBJECTS := app/main.o
app_OBJECTS += app/iapi_from_stream.o
app_OBJECTS += app/backend.o
app_OBJECTS += app/settings.o
app_OBJECTS += app/testconfig.o
app_OBJECTS += app/giveback.o
app_OBJECTS += app/graphenglue.o
app_OBJECTS += app/minipjl.o
app_OBJECTS += app/pjlhandlers.o
app_OBJECTS += app/sniff_di_type.o
app_OBJECTS += app/ltrace_func_map.o

app_INCLUDE  = -I$(topsrcdir)/stubs
app_INCLUDE += -I$(topsrcdir)/ppds
app_INCLUDE += -I$(topscrdir)/html
app_INCLUDE += -I$(topsrcdir)/pcl5
app_INCLUDE += -I$(topsrcdir)/itype
app_INCLUDE += -I$(topsrcdir)/ps2
app_INCLUDE += -I$(topsrcdir)/xl
app_INCLUDE += -I$(topsrcdir)/hex
app_INCLUDE += -I$(topsrcdir)/cfs
app_INCLUDE += -I$(topsrcdir)/ufst4
app_INCLUDE += -I$(topsrcdir)/pclutil
app_INCLUDE += -I$(topsrcdir)/common
app_INCLUDE += $(GRAPHEN_CFLAGS)
app_INCLUDE += $(LIBPAL_CFLAGS)
app_INCLUDE += $(IAPI_CFLAGS)
app_INCLUDE += $(ANRMALLOC_CFLAGS)
app_INCLUDE += $(OPENSSL_CFLAGS)		

$(app_OBJECTS) : INCLUDE += $(app_INCLUDE)

# path to lxkjpg2kservice in cache
ifeq ($(CROSS_COMPILE),)
DEFINES += '-DGJP2KSERVICE_INSTALL_BIN="$(JP2KSERVICE_INSTALL_BIN)"'
else
DEFINES += '-DGJP2KSERVICE_INSTALL_BIN="."'
endif

ifdef USE_LDEBUG
  DEFINES += '-DUSE_LDEBUG=$(USE_LDEBUG)'
endif

# This postscript-specific glue links only into executables that include libps.
app/psglue.o : INCLUDE += $(app_INCLUDE) \
			  -I$(topsrcdir)/ps2/incl \
			  -I$(topsrcdir)/ps2/stubs

app/pdfglue.o : INCLUDE += $(app_INCLUDE) \
              -I$(topsrcdir)/pdf \
              -I$(topsrcdir)/ps2/incl \
              -I$(topsrcdir)/ps2/stubs

# This directimage glue links only into executables that include libps.
app/diglue.o : INCLUDE += $(app_INCLUDE) \
              -I$(topsrcdir)/ps2/incl \
              -I$(topsrcdir)/ps2/stubs

app/htmlglue.o : INCLUDE += $(app_INCLUDE) \
              -I$(topsrcdir)/html \
              -I$(topsrcdir)/ps2/incl \
              -I$(topsrcdir)/ps2/stubs

reports/cfs-files-enumerate.o : INCLUDE += \
                              -I$(topsrcdir)/cfs \
                              -I$(topsrcdir)/pclutil \
                              -I$(topsrcdir)/stubs

pdls_OBJECTS = $(app_OBJECTS) app/psglue.o app/pdfglue.o app/diglue.o app/htmlglue.o
ps_OBJECTS = $(app_OBJECTS) app/psglue.o app/diglue.o
pdfapp_OBJECTS = $(app_OBJECTS) app/psglue.o app/pdfglue.o app/diglue.o
pclxl_OBJECTS = $(app_OBJECTS)
ppdsapp_OBJECTS = $(app_OBJECTS)

cfs-files-enumerate-OBJECTS = reports/cfs-files-enumerate.o

htmlapp_OBJECTS = $(app_OBJECTS) app/htmlglue.o app/psglue.o app/diglue.o

common_libs :=
common_libs += $(HYDRA_LIBSTART)
common_libs +=   $(GRAPHEN_LIBS)
common_libs +=   $(LIBPAL_LIBS)
common_libs +=   $(LIBUFST_LIBS)
common_libs +=   $(LIBITYPE_LIBS)
common_libs +=   $(IAPI_LIBS) 
common_libs += $(ANRMALLOC_LIBS)
common_libs +=   $(LIBTEXT_LIBS)	# XXX we should not be using this
common_libs += $(HYDRA_LIBEND)
common_libs += $(JPEG_LIBS)
common_libs += $(PNG_LIBS)
common_libs += $(TIFF_LIBS)
common_libs += -lm $(ZLIB_LIBS) -ldl $(LDFLAGS)

pclxl_LOADLIBES = -Wl,--whole-archive \
                  $(libpcl5) \
                  $(libitype) \
                  $(libxl) \
                  $(libhex) \
                  $(libdline) \
                  $(libgl) \
                  $(libpclutil) \
                  $(libcommon) \
                  $(liblibc) \
                  $(libmath) \
                  $(libstubs) \
		  $(libcfs) \
                  $(libufst4) \
                  -Wl,--no-whole-archive \
                  libpsstub.a \
                  libpdfstub.a \
		  libppdsstub.a \
                  libhtmlstub.a \
                  $(common_libs)

ps_LOADLIBES = -Wl,--whole-archive \
                  $(libps2) \
                  $(libitype) \
                  $(libcommon) \
                  $(liblibc) \
                  $(libmath) \
                  $(libstubs) \
		  $(libcfs) \
                  $(libufst4) \
                  -Wl,--no-whole-archive \
		  $(LIBT4T6_LIBS) \
                  libpclstub.a \
                  libpdfstub.a \
		  libppdsstub.a \
                  libhtmlstub.a \
		  $(common_libs)

pdfapp_LOADLIBES = -Wl,--whole-archive \
                  $(libpdf) \
                  $(libps2) \
		  $(libitype) \
                  $(libcommon) \
                  $(libdecrypt) \
                  $(liblibc) \
                  $(libmath) \
                  $(libstubs) \
		  $(libcfs) \
                  $(libufst4) \
                  -Wl,--no-whole-archive \
                  $(EXPAT_LIBS) \
                  $(LIBJBIG2_LIBS) \
                  $(CRYPTLIB_LIBS) \
		  $(LIBT4T6_LIBS) \
                  libpclstub.a \
		  libppdsstub.a \
                  libhtmlstub.a \
                  $(common_libs)

ppdsapp_LOADLIBES = -Wl,--whole-archive \
                  $(libppds) \
                  $(libpcl5) \
		  $(libitype) \
                  $(libxl) \
                  $(libhex) \
                  $(libdline) \
                  $(libgl) \
                  $(libpclutil) \
                  $(libcommon) \
                  $(liblibc) \
                  $(libmath) \
                  $(libstubs) \
                  $(libcfs) \
                  $(libufst4) \
                  -Wl,--no-whole-archive \
                  libpclstub.a \
                  libpsstub.a \
                  libpdfstub.a \
		  libhtmlstub.a \
                  $(common_libs)

htmlapp_LOADLIBES = -Wl,--whole-archive \
                  $(libhtml) \
                  $(libps2) \
		  $(libitype) \
                  $(libcommon) \
                  $(liblibc) \
                  $(libmath) \
                  $(libstubs) \
                  $(libcfs) \
                  $(libufst4) \
                  -Wl,--no-whole-archive \
		  $(LIBT4T6_LIBS) \
                  libpclstub.a \
		  libppdsstub.a \
                  libpdfstub.a \
                  $(common_libs)
pdls_LOADLIBES = -Wl,--whole-archive $(libpdls) $(libcfs) \
		  -Wl,--no-whole-archive \
                  $(EXPAT_LIBS) \
                  $(LIBJBIG2_LIBS) \
                  $(CRYPTLIB_LIBS) \
		  $(LIBT4T6_LIBS) \
		  $(common_libs)

cfs-files-enumerate-LOADLIBES = $(libcfs) $(LLIB_LIBS) $(LIBTEXT_LIBS)

#
# Here are a few functions we'll use for linking.
#
filter_lib_names = $(sort $(filter -l%, $1))
filter_lib_files = $(sort $(filter %.a, $1))

#
# This turns $1 into a list of -lNAME and libNAME.a that make can use as
# a dependency listing.  This works with vpath %.a, which we'll set below.
# Use it like this:
#
#    target: $(call depend_on_libs, $(target_LOADLIBES))
#
depend_on_libs = $(call filter_lib_names, $1) $(call filter_lib_files, $1)

#
# Scrape all -L options from $1
#
filter_lib_dirs = $(sort $(patsubst -L%, %, $(filter -L%, $1)))

#
# Add all lib dirs from all targets to the vpath for static libraries,
# so we can let make figure out whether they're old or new.
#
all_lib_dirs := $(call filter_lib_dirs, \
		  $(HYDRA_LIBSTART) $(pclxl_LOADLIBES) $(ppdsapp_LOADLIBES) $(ps_LOADLIBES) \
		  $(htmlapp_LOADLIBES) $(pdfapp_LOADLIBES) $(pdls_LOADLIBES) $(cfs-files-enumerate-LOADLIBES))
vpath %.a $(all_lib_dirs)

# Looks like we need to do that for shared libraries, as well, since we
# don't take any countermeasures to get shared libraries' -lNAMEs out of
# depend_on_libs (since we don't always know which one is which).
vpath %.so $(all_lib_dirs)


#
# Now, we link several programs from the same set of sources, so we have
# to write our own link commands.
#

pclxl: $(pclxl_OBJECTS) $(call depend_on_libs, $(pclxl_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(pclxl_LOADLIBES) $(LDLIBS) $(LD_SO))

ppdsapp: $(ppdsapp_OBJECTS) $(call depend_on_libs, $(ppdsapp_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(ppdsapp_LOADLIBES) $(LDLIBS) $(LD_SO))

ps: $(ps_OBJECTS) $(call depend_on_libs, $(ps_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(ps_LOADLIBES) $(LDLIBS) $(LD_SO))

htmlapp: $(htmlapp_OBJECTS) $(call depend_on_libs, $(htmlapp_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(htmlapp_LOADLIBES)  $(LIBCRYPTO_LIBS) $(LDLIBS) $(LD_SO))

pdfapp: $(pdfapp_OBJECTS) $(call depend_on_libs, $(pdfapp_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(pdfapp_LOADLIBES) $(LDLIBS) $(LD_SO))

pdls: $(pdls_OBJECTS) $(call depend_on_libs, $(pdls_LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(pdls_LOADLIBES)  $(LIBCRYPTO_LIBS) $(LDLIBS) $(LD_SO))

cfs-files-enumerate: $(cfs-files-enumerate-OBJECTS) $(call depend_on_libs, $(cfs-files-enumerate-LOADLIBES))
	$(call link_with_deps, $(LINK.c), $(cfs-files-enumerate-LOADLIBES) $(LDLIBS) $(LD_SO))


TARGETS += pdls pclxl ps pdfapp ppdsapp htmlapp cfs-files-enumerate


CLEANFILES += $(pdls_OBJECTS)
CLEANFILES += pdls.map pclxl.map ps.map pdfapp.map ppdsapp.map html.map cfs-files-enumerate.map


install_programs += pdls
install_programs += cfs-files-enumerate
install_programs += $(topsrcdir)/pdlsrunsuite



pdls.pc : pdls.pc.in
	@echo "Creating $@"
	$Q $(RM) $@
	$Q sed -e 's,@prefix@,$(prefix),g' \
		< $< > $@

CLEANFILES += pdls.pc
install_pkgconfigdir += pdls.pc
