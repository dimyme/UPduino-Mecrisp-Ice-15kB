UPduino Mecrisp-Ice with full 15kB block ram.
---------------------------------------------

An Experimental version of the Mecrisp-Ice Forth running on the J1a 16bit processor.

This is an IceStorm version for UP5k/15kB ready to build.

It does not require any other tools to be installed except the latest IceStorm tools.

The ram source contains the nucleus.fs image (from Mecrisp-Ice).

The sizes are 541-557 PLBs, and the timing estimates 19-22MHz based on the arachne's seed.

Build it with the "compile" script. You will get the bitstream binary "j1a0.bin".

Works at 20MHz external xtal oscillator fine.

Does not work at 30MHz yet..


IgorM, 18 Jan 2018