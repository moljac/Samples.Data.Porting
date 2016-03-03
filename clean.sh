#!/bin/bash

find \
	./ \
	-type d \
		\( \
			-name "*.dump" \
		\) \
	-not -path "*Dropbox*" \
	-not -path "*Google Drive*" \
	-exec rm -rf {} \;
