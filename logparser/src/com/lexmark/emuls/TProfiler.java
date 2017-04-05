package com.lexmark.emuls;

import com.lexmark.emuls.profiler.FunctionLoader;
import com.lexmark.emuls.profiler.SentryLogParser;
import com.lexmark.emuls.profiler.TImporter;

public class TProfiler extends FolderWatcherImpl {

	public TProfiler(String ftpHome) throws Exception {
		super(ftpHome, TImporter.getInstance());
	}
	
	public static void main(String[] args) {
		try {
			boolean host = false;
			boolean user = false;
			boolean pass = false;
			boolean watch = false;
			boolean checksum = false;
			
			String hostname = "";
			String username = "";
			String passwd = "";
			String tracker = "";
			String ftpHome = "";
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
				
				if (s != null && s.equalsIgnoreCase("-c")) {
					checksum = true;
					continue;
				}
				if (checksum) {
					System.out.println(String.format("Processing checksum: %s\n", s));
					TImporter.checksum_update = Boolean.parseBoolean(s);
					checksum = false;
					continue;
				}
			}
			
			if (null != tracker && tracker.equalsIgnoreCase("sentry")) {
				ftpHome = System.getenv("SENTRY_LOGS_DIR");
				instance = new SentryLogWatcher(ftpHome, hostname, username, passwd);
			}
			else if (null != tracker && tracker.equalsIgnoreCase("funcs")) {
				ftpHome = System.getenv("FUNCS_DIR");
				FunctionLoader loader = FunctionLoader.getInstance();
				instance = new FolderWatcherImpl(ftpHome, loader);
			}
			else {
				ftpHome = System.getenv("FTP_HOME");
				instance = new TProfiler(ftpHome);
			}
			System.out.format("%s: monitoring location: %s, db: %s\n", 
					instance.getClass().getSimpleName(), ftpHome, host);
			instance.start(hostname, username, passwd);
			
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
