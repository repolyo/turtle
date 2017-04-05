package com.lexmark.emuls;

import com.lexmark.emuls.profiler.DBConnection;

public class FolderWatcherImpl extends FolderWatcher {
	
	public FolderWatcherImpl(String ftpHome, DBConnection parser) {
		super(ftpHome, parser);
	}
	
	@Override
	protected boolean importFile(String file) throws Exception {
		return parser.importFile(file);
	}
}
