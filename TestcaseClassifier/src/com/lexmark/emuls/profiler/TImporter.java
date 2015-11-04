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
    private static String FUNC_HIT = "(.*)\\s+(.*)";
    
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
				                	System.out.println(line);
				            	}
				                else {
				                	Matcher m = Pattern.compile(FUNC_HIT).matcher(line);
				                	if (m.matches()) {
				                		System.out.println(line);
				                        System.out.println("Found value: " + m.group(1));
				                        System.out.println("Found value: " + m.group(2));
				                    }
				                	break;
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
