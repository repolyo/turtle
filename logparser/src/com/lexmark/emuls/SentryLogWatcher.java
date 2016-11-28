package com.lexmark.emuls;

import com.lexmark.emuls.profiler.SentryLogParser;

public class SentryLogWatcher extends FolderWatcher {

	public SentryLogWatcher(String ftpHome, String host, String user, String pass) {
		super(ftpHome);
		SentryLogParser.dbHost = host;
		SentryLogParser.dbUser = user;
		SentryLogParser.dbPasswd = pass;
		
		System.out.format("%s: monitoring location: %s, db: %s\n", 
				this.getClass().getSimpleName(), ftpHome, host);
	}
	
	@Override
	protected boolean importFile(String file) throws Exception {
		SentryLogParser.importFile(file);
		return true;
	}
}
