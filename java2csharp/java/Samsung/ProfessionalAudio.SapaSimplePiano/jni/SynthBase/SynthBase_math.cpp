
#include "SynthBase_math.h"

const float EXP_T[(NOTE_RANGE_POG - NOTE_RANGE_NEG)+1] = {
#include "exp.txt"
};

const float POW_T[NOTE_RANGE_POG][REAL_RANGE_POS*2+1] = {
#include "pow.txt"
};

/* pitch wheel value 14bit value 0x0000 to 0x3ffff centor 0x2000 -> synthbase 0x00 to 0xff center 0x80(128) */
const float PITCH_WHEEL_T[256] = {
#include "pitch_wheel.txt"
};

const float pan_tab[PAN_SIZE] = {
#include "pan_tab.txt"
};

const float Attenuation_tab[ATTENUATION_SIZE] = {
#include "attenuation.txt"
};

void pan_gain_ref(int c, float &l, float &r )
{
  int index;
  index = c + (PAN_RIGHT);
  if (index < 0) {
    index = 0;
  } else if (index > PAN_SIZE -1 ) {
    index = PAN_SIZE - 1;
  } 

  r = pan_tab[(int) index];
  l = pan_tab[(int) (PAN_SIZE - index -1)];
}
