#!/bin/bash
PYTHON=
BINDIR=/usr/bin
export OTHERARGS="-d halftone-color" ; export SCREEN_CONTROL_FILE=/usr/share/graphen/screens/color/screen.conf ; export TCASE_MOUNT=$TCASES_MOUNT ; export XITHREADCOUNT=1

DIR=$(dirname -- $(readlink -f -- "$0"))

FILENAME=`basename $0`
NAME=${FILENAME%.sh}

MASTERS=$DIR/$NAME.checksum
eval $TEST_LOCATION/Rendering/simrunner.py $DEVICE -c $MASTER $@
