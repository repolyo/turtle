#!/usr/bin/env python

# manually defined offset address serve as marking
library_offset = {
        'libprintservice.so'                :'10000000000',
        'libprintservice_realtime.so'       :'20000000000',
        'libprintservice_graphenbackend.so' :'30000000000',
        'libremotejobsystem.so'             :'40000000000'
        }

# Add subsystems to trace here -- add at the end prefix with '|<subsystem>'
lib_subsystems = [
        'printservice',
        '|jobsystem',
        '|pullprint'
        ]
