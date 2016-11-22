package com.lexmark.emuls;
import static java.nio.file.StandardWatchEventKinds.*;

import java.io.IOException;
import java.nio.file.FileSystems;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.WatchEvent;
import java.nio.file.WatchKey;
import java.nio.file.WatchService;

import com.lexmark.emuls.profiler.TImporter;

public class TProfiler {

	public static void main(String[] args) {
		try {
		
		// TODO Auto-generated method stub
		WatchService watcher = FileSystems.getDefault().newWatchService();
		
		String ftpHome = System.getenv("FTP_HOME");
		Path dir = FileSystems.getDefault().getPath(ftpHome);
		if (!Files.exists(dir)) Files.createDirectories(dir);
		
	    WatchKey key = dir.register(watcher,
	                           ENTRY_CREATE);
	    
	    boolean host = false;
	    boolean user = false;
	    boolean pass = false;
	    for (String s: args) {
	    	if (s != null && s.equalsIgnoreCase("-h")) {
	    		host = true;
	    		continue;
	    	}
	    	if (host) {
	    		TImporter.dbHost = s;
	    		host = false;
	    		continue;
	    	}
	    	
	    	if (s != null && s.equalsIgnoreCase("-u")) {
	    		user = true;
	    		continue;
	    	}
	    	if (user) {
	    		TImporter.dbUser = s;
	    		user = false;
	    		continue;
	    	}
	    	
	    	if (s != null && s.equalsIgnoreCase("-p")) {
	    		pass = true;
	    		continue;
	    	}
	    	if (pass) {
	    		TImporter.dbPasswd = s;
	    		pass = false;
	    		continue;
	    	}
        }
	    System.out.format("Turtle: monitoring location: %s, db: %s\n", ftpHome, TImporter.dbHost);
	    
		for (;;) {

		    // wait for key to be signaled
		    try {
		        key = watcher.take();
		    } catch (InterruptedException x) {
		        return;
		    }

		    for (WatchEvent<?> event: key.pollEvents()) {
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
		        WatchEvent<Path> ev = (WatchEvent<Path>)event;
		        Path filename = ev.context();

//		        if (kind == ENTRY_MODIFY) {
//		        	System.out.format("modified file: %s, skipping... %n", filename);
////		        	new File(filename.toString()).delete();
//		        	continue;
//		        }
		        try {
		        	TImporter.importFile(String.format("%s/%s",dir, filename));
			    } catch (Exception e) {
        			e.printStackTrace();
			    }
		    }

		    // Reset the key -- this step is critical if you want to
		    // receive further watch events.  If the key is no longer valid,
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
