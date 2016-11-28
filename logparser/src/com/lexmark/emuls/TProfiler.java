package com.lexmark.emuls;

import com.lexmark.emuls.profiler.SentryLogParser;
import com.lexmark.emuls.profiler.TImporter;

public class TProfiler extends FolderWatcher {

	public TProfiler(String ftpHome, String host, String user, String pass) {
		super(ftpHome);
		TImporter.dbHost = host;
		TImporter.dbUser = user;
		TImporter.dbPasswd = pass;
		
		System.out.format("%s: monitoring location: %s, db: %s\n", 
				this.getClass().getSimpleName(), ftpHome, host);
	}
	
	@Override
	protected boolean importFile(String file) throws Exception {
		TImporter.importFile(file);
		return true;
	}
	
	public static void main(String[] args) {
		boolean host = false;
		boolean user = false;
		boolean pass = false;
		boolean watch = false;
		
		String hostname = "";
		String username = "";
		String passwd = "";
		String tracker = "";
		FolderWatcher instance = null;
		
		for (String s : args) {
			if (s != null && s.equalsIgnoreCase("-h")) {
				host = true;
				continue;
			}
			if (host) {
				hostname = s;
				host = false;
				continue;
			}

			if (s != null && s.equalsIgnoreCase("-u")) {
				user = true;
				continue;
			}
			if (user) {
				username = s;
				user = false;
				continue;
			}

			if (s != null && s.equalsIgnoreCase("-p")) {
				pass = true;
				continue;
			}
			if (pass) {
				passwd = s;
				pass = false;
				continue;
			}
			
			if (s != null && s.equalsIgnoreCase("-w")) {
				watch = true;
				continue;
			}
			if (watch) {
				tracker = s;
				watch = false;
				continue;
			}
		}
		
		if (null != tracker && tracker.equalsIgnoreCase("sentry")) {
			instance = new SentryLogWatcher(System.getenv("SENTRY_LOGS_DIR"), hostname, username, passwd);
		}
		else {
			instance = new TProfiler(System.getenv("FTP_HOME"), hostname, username, passwd);
		}
		instance.run();
	}
}
