#define FD_SETSIZE 1024
#define NFDBITS (8 * sizeof(unsigned long))
#define __FDSET_LONGS (FD_SETSIZE/NFDBITS)
#define __FDMASK(fd) (1UL << ((fd) % NFDBITS))

#define __FDELT(fd) ((fd) / NFDBITS)

#define __FDS_BITS(set) (((fd_set*)(set))->fds_bits)

#define FD_SET(fd, set) (__FDS_BITS(set)[__FDELT(fd)] |= __FDMASK(fd))
#define FD_CLR(fd, set) (__FDS_BITS(set)[__FDELT(fd)] &= ~__FDMASK(fd))
#define FD_ISSET(fd, set) ((__FDS_BITS(set)[__FDELT(fd)] & __FDMASK(fd)) != 0)

#define FD_ZERO(set) \
  do { \
    size_t __i; \
    for (__i = 0; __i < __FDSET_LONGS; ++__i) { \
      (set)->fds_bits[__i] = 0; \
    } \
  } while (0)

typedef struct {
	unsigned long fds_bits[__FDSET_LONGS];
} fd_set;
