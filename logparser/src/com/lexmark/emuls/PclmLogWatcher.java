package com.lexmark.emuls;

import com.lexmark.emuls.profiler.PclmLogger;

public class PclmLogWatcher extends FolderWatcherImpl {

	public PclmLogWatcher(String ftpHome, String host, String user, String pass) throws Exception {
		super(ftpHome, PclmLogger.getInstance());
	}
}
