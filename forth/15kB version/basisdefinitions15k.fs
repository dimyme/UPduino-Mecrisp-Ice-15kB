\ Definitions in high-level Forth that can be compiled by the small
\ nucleus itself. They are not included into the bitstream for default.
\ From Mecrisp-Ice repo

\ Modification and Experiments for UPduino Board by IgorM 11 Dec 2017, 15kB version!

: [']
    ' postpone literal
; immediate 0 foldable

: [char]
    char postpone literal
; immediate 0 foldable

: (
    [char] ) parse 2drop
; immediate 0 foldable

: u>= ( u1 u2 -- ? ) u< invert ; 2 foldable
: u<= ( u1 u2 -- ? ) u> invert ; 2 foldable
: >=  ( n1 n2 -- ? )  < invert ; 2 foldable
: <=  ( n1 n2 -- ? )  > invert ; 2 foldable

: else
    postpone ahead
    swap
    postpone then
; immediate

: while
    postpone if
    swap
; immediate

: repeat
     postpone again
     postpone then
; immediate

: create
    :
    here 4 + postpone literal
    postpone ;
;

: >body
    @ 32767 and
;

: m*
    2dup xor >r
    abs swap abs um*
    r> 0< if dnegate then
; 2 foldable

: variable
    create ,
    0 foldable
;

: constant  : postpone literal postpone ; 0 foldable ;

: sgn ( u1 n1 -- n2 ) \ n2 is u1 with the sign of n1
    0< if negate then
; 2 foldable

\ Divide d1 by n1, giving the symmetric quotient n3 and the remainder
\ n2.
: sm/rem ( d1 n1 -- n2 n3 )
    2dup xor >r     \ combined sign, for quotient
    over >r         \ sign of dividend, for remainder
    abs >r dabs r>
    um/mod          ( remainder quotient )
    swap r> sgn     \ apply to remainder
    swap r> sgn     \ apply to quotient
; 3 foldable

\ Divide d1 by n1, giving the floored quotient n3 and the remainder n2.
\ Adapted from hForth
: fm/mod ( d1 n1 -- n2 n3 )
    dup >r 2dup xor >r
    >r dabs r@ abs
    um/mod
    r> 0< if
        swap negate swap
    then
    r> 0< if
        negate         \ negative quotient
        over if
            r@ rot - swap 1-
        then
    then
    r> drop
; 3 foldable

: */mod     >r m* r> sm/rem ; 3 foldable
: */        */mod nip ; 3 foldable

: spaces
    begin
        dup 0>
    while
        space 1-
    repeat
    drop
;

: roll
    ?dup if
        swap >r
        1- recurse
        r> swap
    then
;

: pick
    ?dup if
        swap >r
        1- recurse
        r> swap
    else
        dup
    then
;

create BUF0
16 cells 2 + 128 max
allot here constant BUF

0 variable hld

: <#
    BUF hld !
;

: hold
    hld @ 1- dup hld ! c!
;

: sign
    0< if
        [char] - hold
    then
;

: #
    0 base @ um/mod >r base @ um/mod swap
    9 over < [ char A char 9 1 + - ] literal and +
    [ char 0 ] literal + hold r>
;

: #s
    begin
        #
        2dup d0=
    until
;

: #>
    2drop hld @ BUF over -
;

: (d.)
    dup >r dabs <# #s r> sign #>
;

: ud.
    <# #s #> type space
;

: d.
    (d.) type space
;

: .
    s>d d.
;

: u.
    0 d.
;

: rtype ( caddr u1 u2 -- ) \ display character string specified by caddr u1
                           \ in a field u2 characters wide.
  2dup u< if over - spaces else drop then
  type
;

: d.r
    >r (d.)
    r> rtype
;

: .r
    >r s>d r> d.r
;

: u.r
    0 swap d.r
;

( Memory operations                          JCB 18:02 05/31/15)

: move \ ( addr1 addr2 u -- )
    >r 2dup u< if
        r> cmove>
    else
        r> cmove
    then
;

: /mod      >r s>d r> sm/rem ; 2 foldable
: /         /mod nip ; 2 foldable
: mod       /mod drop ; 2 foldable

: ."
    [char] " parse
    state @ if
        postpone sliteral
        postpone type
    else
        type
    then
; immediate 0 foldable

\ #######   CORE EXT   ########################################

: unused
    $3C00 here -
;

: pad
    here aligned
;

: within    over - >r - r> u< ; 3 foldable

: s"
    [char] " parse
    state @ if
        postpone sliteral
    then
; immediate

( CASE                                       JCB 09:15 07/18/14)
\ From ANS specification A.3.2.3.2

: case 0 ; immediate  ( init count of ofs )

: of  ( #of -- orig #of+1 / x -- )
    1+    ( count ofs )
    >r    ( move off the stack in case the control-flow )
          ( stack is the data stack. )
    postpone over  postpone = ( copy and test case value)
    postpone if    ( add orig to control flow stack )
    postpone drop  ( discards case value if = )
    r>             ( we can bring count back now )
; immediate

: endof ( orig1 #of -- orig2 #of )
    >r   ( move off the stack in case the control-flow )
         ( stack is the data stack. )
    postpone else
    r>   ( we can bring count back now )
; immediate

: endcase  ( orig1..orign #of -- )
    postpone drop  ( discard case value )
    0 ?do
      postpone then
    loop
; immediate

: ms   ( u -- ) 0 do 2727 0 do loop loop ; \ 11 cycles per loop run.. For 30 MHz / 11 = 2727

: timer1 ( ud -- ) swap 110 io! 111 io! ;  \ load timer1 with a double value, counts down

: now   ( -- ) 0 109 io! ;
: ticksl ( -- u ) 105 io@ ;
: ticksh ( -- u ) 106 io@ ;
: tickshh ( -- u ) 107 io@ ;

\ : delay ( u -- ) begin dup ticks u< until drop ;

: randombit ( -- 0 | 1 ) $2000 io@ $20 and 5 rshift ;
: random ( -- x ) 0  16 0 do 2* randombit or 100 0 do loop loop ;

\ Interrupts -  Enable/Disable INT Mask register and INT Flags handling

: intmask! ( mask -- ) 50 io! ;
: intmask@ ( -- mask ) 50 io@ ;

: intflag! ( mask -- ) 40 io! ;
: intflag@ ( -- mask ) 40 io@ ;

\ For 15kB of ram, interrupt vectors placed at the end of ram
$608C $3BFE ! \ Location $3BFE is an interrupt vector ! Place ALU exit opcode here.
$608C $3BFC ! \ Location $3BFC is an interrupt vector ! Place ALU exit opcode here.
$608C $3BFA ! \ Location $3BFA is an interrupt vector ! Place ALU exit opcode here.
$608C $3BF8 ! \ Location $3BF8 is an interrupt vector ! Place ALU exit opcode here.
$608C $3BF6 ! \ Location $3BF6 is an interrupt vector ! Place ALU exit opcode here.
$608C $3BF4 ! \ Location $3BF4 is an interrupt vector ! Place ALU exit opcode here.
$608C $3BF2 ! \ Location $3BF2 is an interrupt vector ! Place ALU exit opcode here.
$608C $3BF0 ! \ Location $3BF0 is an interrupt vector ! Place ALU exit opcode here.

: 2variable
    create 2 cells allot
;

: 2constant
    create , ,
    does> 2@
;

: tnegate                           ( t . . -- -t . . )
    >r  2dup or dup if drop  dnegate 1  then
    r> +  negate ;

: 2>r   r> -rot swap >r >r >r ;

: 2r>
    postpone r>
    postpone r>
    postpone swap
; immediate

: 2r@   r> 2r> 2dup 2>r rot >r ;

: t*                                ( d . n -- t . . )
                                    ( d0 d1 n)
    2dup xor >r                     ( r: sign)
    >r dabs r> abs
    2>r                             ( d0)( r: sign d1 n)
    r@ um* 0                        ( t0 d1 0)
    2r> um*                         ( t0 d1 0 d1*n .)( r: sign)
    d+                              ( t0 t1 t2)
    r> 0< if tnegate then ;

: t/                                ( t . . u -- d . )
                                    ( t0 t1 t2 u)
    over >r >r                      ( t0 t1 t2)( r: t2 u)
    dup 0< if tnegate then
    r@ um/mod                       ( t0 rem d1)
    rot rot                         ( d1 t0 rem)
    r> um/mod                       ( d1 rem' d0)( r: t2)
    nip swap                        ( d0 d1)
    r> 0< if dnegate then ;

: m*/  ( d . n u -- d . )  >r t*  r> t/ ;

\ : test 0 do cr now ticksl ticksh tickshh 30000 t/ 2000 ms now ticksl ticksh tickshh 30000 t/ 2swap d- d. cr loop ;

\ #######   Flash   ###########################################

\ Save memory image to SPI Flash - UPduino, N25Q032A 4MB SPI flash

: waitspi  \ changes for UPduino board, IgorM
  begin
    $05 >spi \ Read Flag status register
    spi> $01 and 0= \ WIP: Write in Progress.
    idle
  until
;

: spiwe 
    $06 >spi \ Write enable
    idle
;

: erase 
  dup 2 u> if \ Never overwrite bitstream !
    $AB >spi \ Release from Deep Power Down, IgorM
	idle
    0  begin 1+ dup 500 =  until drop  \ delay ~100us
    spiwe
    $D8 >spi \ Sector erase
        >spi  \ Sector number
    $00 >spi   \ Address high
    $00 >spi    \ Address low
    idle
    waitspi
  else drop then
;

: save 
  dup 2 u> if \ Never overwrite bitstream !

    dup erase
    0      \ 
    begin
      spiwe

      $02 >spi \ Page program
     over >spi  \ Sector number
     dup
 8 rshift >spi   \ Address high
      $00 >spi    \ Address low

      begin
        dup c@ >spi
        1+
        dup $FF and 0=
      until

      idle
      waitspi

      dup $3C00 =    \ for 15kB ram
    until
    2drop

  else drop then \ Bitstream protection
;

: marker
  create
    forth 2@        \ preserve FORTH and DP after this
    , 2 cells + ,
  does>
    2@ forth 2! \ restore FORTH and DP
;

marker new

\ #######   DUMP   ############################################

: dump
    ?dup
    if
        1- 4 rshift 1+
        0 do
            cr dup dup .x space space
            16 0 do
                dup c@ .x2 1+
            loop
            space swap
            16 0 do
                dup c@ dup bl 127 within invert if
                    drop [char] .
                then
                emit 1+
            loop
            drop
        loop
    then
    drop
;

\ #######   INSIGHT   #########################################


( Deep insight into stack, dictionary and code )
( Matthias Koch )

: .s ( -- )
  \ Save initial depth
  depth dup >r

  \ Flush stack contents to temporary storage
  begin
    dup
  while
    1-
    swap
    over cells pad + !
  repeat
  drop

  \ Print original depth
  ." [ "
  r@ .x2
  ." ] "

  \ Print all elements in reverse order
  r@
  begin
    dup
  while
    r@ over - cells pad + @ .x
    1-
  repeat
  drop

  \ Restore original stack
  0
  begin
    dup r@ u<
  while
    dup cells pad + @ swap
    1+
  repeat
  rdrop
  drop
;

: insight ( -- )  ( Long listing of everything inside of the dictionary structure )
    base @ hex cr
    forth @
    begin
        dup
    while
         ." Addr: "     dup .x
        ."  Link: "     dup @ 2/ 2/ $3FFE and .x
        ."  Flags: "    dup 2 + c@ 128 and if ." I " else ." - " then
                        dup @ 7 and ?dup if 1- u. else ." - " then
        ."  Code: "     dup 2 + count 127 and + aligned .x
        space           dup 2 + count 127 and type
        @ 2/ 2/ $3FFE and cr
    repeat
    drop
    base !
;

0 variable disasm-$    ( Current position for disassembling )
0 variable disasm-cont ( Continue up to this position )

: name. ( Address -- )  ( If the address is Code-Start of a dictionary word, it gets named. )

  dup ['] s, 24 + = \ Is this a string literal ?
  if
    ."   --> s" [char] " emit space
    disasm-$ @ count type
    [char] " emit

    disasm-$ @ c@ 1+ aligned disasm-$ +!
    drop exit
  then

  >r
  forth @
  begin
    dup
  while
    dup 2 + count 127 and + aligned ( Dictionary Codestart )
      r@ = if ."   --> " dup 2 + count 127 and type then
    @ 2/ 2/ $3FFE and
  repeat
  drop r>

  $000C =                            \ A call to execute
  disasm-$ @ 4 - @ $C000 and $C000 =  \ after a literal which has bit $4000 set means:
  and                                  \ Memory fetch.
  if
    ."   --> " disasm-$ @ 4 - @ $3FFF and .x ." @"
  then
;

: alu. ( Opcode -- ) ( If this opcode is from an one-opcode definition, it gets named. This way inlined ALUs get a proper description. )

  dup $6127 = if ." >r"    drop exit then
  dup $6B11 = if ." r@"    drop exit then
  dup $6B1D = if ." r>"    drop exit then
  dup $600C = if ." rdrop" drop exit then

  $FF73 and
  >r
  forth @
  begin
    dup
  while
    dup 2 + count 127 and + aligned @ ( Dictionary First-Opcode )
        dup $E080 and $6080 =
        if
          $FF73 and r@ = if rdrop 2 + count 127 and type space exit then
        else
          drop
        then

    @ 2/ 2/ $3FFE and
  repeat
  drop r> drop
;

: memstamp ( Addr -- ) dup .x ." : " @ .x ."   " ; ( Shows a memory location nicely )

: disasm-step ( -- )
  disasm-$ @ memstamp
  disasm-$ @ @  ( Fetch next opcode )
  2 disasm-$ +! ( Increment position )

  dup $8000 and         if ." Imm  " $7FFF and    dup .x 6 spaces                      .x       exit then ( Immediate )
  dup $E000 and $0000 = if ." Jmp  " $1FFF and 2* dup                                  .x name. exit then ( Branch )
  dup $E000 and $2000 = if ." JZ   " $1FFF and 2* disasm-cont @ over max disasm-cont ! .x       exit then ( 0-Branch )
  dup $E000 and $4000 = if ." Call " $1FFF and 2* dup                                  .x name. exit then ( Call )
                           ." Alu"   13 spaces dup alu. $80 and if ." exit" then                          ( ALU )
;

: seec ( -- ) ( Continues to see )
  base @ hex cr
  0 disasm-cont !
  begin
    disasm-$ @ @
    dup  $E080 and $6080 =           ( Loop terminates with ret )
    swap $E000 and 0= or             ( or when an unconditional jump is reached. )
    disasm-$ @ disasm-cont @ u>= and ( Do not stop when there has been a conditional jump further )

    disasm-step cr
  until

  base !
;

: see ( -- ) ( Takes name of definition and shows its contents from beginning to first ret )
  ' disasm-$ !
  seec
;

