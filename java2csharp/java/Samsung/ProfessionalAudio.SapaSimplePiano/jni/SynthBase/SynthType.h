#ifndef SYSTH_TYPE_H
#define SYSTH_TYPE_H

typedef char int8;
typedef unsigned char uint8;
typedef unsigned char uchar;

typedef short int16;
typedef unsigned short uint16;

typedef long int32;
typedef unsigned long uint32;	

#ifndef NULL
#ifdef __cplusplus
#define NULL    0
#else
#define NULL    ((void *)0)
#endif
#endif

#endif