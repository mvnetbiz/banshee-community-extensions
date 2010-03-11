AC_DEFUN([BCE_U1MS],
[
	BCE_ARG_DISABLE([UbuntuOneMusicStore], [no])

        BCE_CHECK_EXTENSION_DEP([UbuntuOneMusicStore], [BANSHEE_155],
                [banshee-1-thickclient >= 1.5.5],
                [Banshee 1.5.5 was not found. Please install it or disable the UbuntuOneMusicStore extension by passing --disable-ubuntuonemusicstore])

	BCE_CHECK_EXTENSION_DEP([UbuntuOneMusicStore], [UBUNTUONESHARP],
		[ubuntuone-sharp-1.0],
		[UbuntuOne-sharp was not found. Please install it or disable the UbuntuOneMusicStore extension by passing --disable-ubuntuonemusicstore])

	if test "x$enable_UbuntuOneMusicStore" = "xtry" \
		&& test "x$have_UBUNTUONESHARP" = "xyes"; then
		enable_UbuntuOneMusicStore=yes
	fi

	if test "x$enable_UbuntuOneMusicStore" = "xyes"; then
		AM_CONDITIONAL(ENABLE_U1MS, true)
	else
		AM_CONDITIONAL(ENABLE_U1MS, false)
	fi
])
