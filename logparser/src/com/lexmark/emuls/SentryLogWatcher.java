package com.lexmark.emuls;

import com.lexmark.emuls.profiler.SentryLogParser;

public class SentryLogWatcher extends FolderWatcherImpl {

	public SentryLogWatcher(String ftpHome, String host, String user, String pass) throws Exception {
		super(ftpHome, SentryLogParser.getInstance());
	}
}
