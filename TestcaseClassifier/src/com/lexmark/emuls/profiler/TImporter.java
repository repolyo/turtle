package com.lexmark.emuls.profiler;
import java.io.File;
import java.io.FileNotFoundException;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Scanner;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class TImporter extends LinkedList<String> {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	static String newLine = System.getProperty("line.separator");
	private static TImporter queue = null;
	private static boolean busy = false;
	
	// invoking = ./pdls -e PDF ~/testcases/hello_lexmark.pdf
	private static String TESTCASE = "^invoking =.*";
	
	// [/bonus/scratch/tanch/pdls/app/main.c:631] => main
    private static String FUNC_HIT = "^\\[.*(\\/.*\\/.*\\/.*):(\\d+)\\]\\s+=>\\s+(.*)";
    
	private TImporter() { }
	
	public static TImporter getInstance() {
		if (null == queue) {
			queue = new TImporter();
		}
		return queue;
	}
	
	public static boolean importFile(String filename) {
		final Queue<String> q = getInstance();
		synchronized (q) {
			q.add(filename);
		}
		if (!busy) {
			Thread t = new Thread(new Runnable() {
				public void run() {
					busy = true;
					do {
						try {
							String testResult = null;
							synchronized (q) {
								testResult = q.poll();
							}
							
							File file = new File(testResult);
							Scanner scanner = new Scanner(file);
				            while (scanner.hasNextLine()) {
				                String line = scanner.nextLine();
				                if(line.matches(TESTCASE)) {
				                	System.out.println("SOURCE: " + line);
				            	}
				                else {
				                	Pattern re = Pattern.compile(FUNC_HIT);
				                	Matcher m = re.matcher(line);
				                	if ( m.find() ) {
				                	  String sql = String.format("INSERT INTO FUNC(source_file, line_no, func_name) VALUES('%s', %s, '%s');", 
				                			  m.group(1), m.group(2), m.group(3));
				                	  System.out.println(sql);
			                	    }
				                }
				            }
				            
				            scanner.close();
				            System.out.println(newLine);
				            System.out.println(newLine);
	//			            Thread.yield();
						}
						catch (FileNotFoundException e) {
				            e.printStackTrace();
				        }
						busy = !q.isEmpty();
					} while (busy);
				}
			});
			t.setDaemon(true);
			t.start();
		}
	    return busy;
	}
}
