package com.lexmark.emuls;

import static java.nio.file.StandardWatchEventKinds.ENTRY_CREATE;
import static java.nio.file.StandardWatchEventKinds.OVERFLOW;

import java.io.IOException;
import java.nio.file.FileSystems;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.WatchEvent;
import java.nio.file.WatchKey;
import java.nio.file.WatchService;

import com.lexmark.emuls.profiler.DBConnection;
import com.lexmark.emuls.profiler.SentryLogParser;

public abstract class FolderWatcher implements Runnable {
	private String ftpHome;
	protected DBConnection parser;
	
	public FolderWatcher(String ftpHome, DBConnection parser) {
		this.ftpHome = ftpHome;
		this.parser = parser;
	}

	protected abstract boolean importFile(String file) throws Exception;
	
	public void start(String host, String user, String pass) {
		this.parser.setDbHost(host);
		this.parser.setDbUser(user);
		this.parser.setDbPasswd(pass);
		this.run();
	}
	
	@Override
	public void run() {
		try {
			WatchService watcher = FileSystems.getDefault().newWatchService();
			Path dir = FileSystems.getDefault().getPath(ftpHome);
			if (!Files.exists(dir))
				Files.createDirectories(dir);

			WatchKey key = dir.register(watcher, ENTRY_CREATE);
			String filename = null;
			for (;;) {

				// wait for key to be signaled
				try {
					key = watcher.take();
				} catch (InterruptedException x) {
					return;
				}

				for (WatchEvent<?> event : key.pollEvents()) {
					WatchEvent.Kind<?> kind = event.kind();

					// This key is registered only
					// for ENTRY_CREATE events,
					// but an OVERFLOW event can
					// occur regardless if events
					// are lost or discarded.
					if (kind == OVERFLOW) {
						continue;
					}

					// The filename is the
					// context of the event.
					WatchEvent<Path> ev = (WatchEvent<Path>) event;
					String path = String.format("%s/%s", dir, ev.context());
					if (path.endsWith(SentryLogParser.LOG_EXT)) {
						continue; // our logparser scan log, skip
					}
					else if (null != filename && path.contains(filename)) {
						continue;
					}
					filename = path;
					// if (kind == ENTRY_MODIFY) {
					// System.out.format("modified file: %s, skipping... %n",
					// filename);
					//// new File(filename.toString()).delete();
					// continue;
					// }
					try {
						importFile (filename);
					} catch (Exception e) {
						e.printStackTrace();
					}
				}

				// Reset the key -- this step is critical if you want to
				// receive further watch events. If the key is no longer valid,
				// the directory is inaccessible so exit the loop.
				boolean valid = key.reset();
				if (!valid) {
					break;
				}
			}

		} catch (IOException x) {
			System.err.println(x);
		}
	}
}
