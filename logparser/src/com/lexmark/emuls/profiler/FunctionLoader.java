package com.lexmark.emuls.profiler;

import java.io.File;
import java.io.FileNotFoundException;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.util.Queue;
import java.util.Scanner;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.omg.PortableInterceptor.ORBInitInfoPackage.DuplicateName;

class Emulator
{
	String name;
	int funcs;
	
	Emulator (String name) {
		this.name = name;
		this.funcs = 0;
	}
	
	int Filter(String str) {
		if (-1 != str.indexOf(name)) {
			funcs++;
		}
		return funcs;
	}
	
	public String toString() {
		return "" + name + ": " + funcs;
	}
}

public class FunctionLoader extends DBConnection {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	protected static boolean busy = false;
	
	// [/bonus/scratch/tanch/pdls/app/main.c:631] => main
	private static String FUNC_HIT = "^(.*)\\s[t|T]\\s+(.*)\\s+(\\/.*\\/.*\\/.*):(\\d+)";
	
	// 083f6194 T xiPolyPath	/usr/src/debug/graphen/0.0+gitAUTOINC+4340ab36d0-r0/git/xi/paths.c:3993
	static Pattern func = Pattern.compile(FUNC_HIT);
	
	protected FunctionLoader() throws Exception {
		super();
	}

	public static FunctionLoader getInstance() throws Exception { 
		if (null == instance) {
			instance = new FunctionLoader();
		}
		return (FunctionLoader)instance;
	}
	
	public boolean importFile(String file) throws Exception { 
		final Queue<String> q = getInstance();
		synchronized (q) {
			if (q.contains(file)) {
				throw new DuplicateName("already is processing: " + file);
			}
			q.add(file);
		}
		if (!busy) {
			Thread t = new Thread(new Runnable() {
			public void run() {
				int total = 0;
				Emulator [] emuls = new Emulator[] {
						new Emulator("/bonus/scratch/tanch/pdls/cfs"),
						new Emulator("/bonus/scratch/tanch/pdls/pdf"),
						new Emulator("/bonus/scratch/tanch/pdls/xl"),
						new Emulator("/bonus/scratch/tanch/pdls/ps2"),
						new Emulator("/bonus/scratch/tanch/pdls/pcl5"),
						new Emulator("/bonus/scratch/tanch/pdls/gl"),
						new Emulator("/bonus/scratch/tanch/pdls/ppds"),
						new Emulator("/bonus/scratch/tanch/pdls/html"),
						new Emulator("/bonus/scratch/tanch/pdls/hex"),
						new Emulator("/bonus/scratch/tanch/pdls/rom_fs2"),
						new Emulator("/usr/src/debug/graphen/"),
						new Emulator("/usr/src/debug/ufst/"),
				};
				busy = true;
				
				Connection conn = null;
				PreparedStatement stmt2 = null;
				try {
					conn = getDbConn();
					stmt2 = conn.prepareStatement("MERGE INTO FUNC f using dual on (source_file=? AND func_name = ?)" +
            				" WHEN NOT matched then INSERT (SOURCE_FILE, LINE_NO, FUNC_NAME) values (?,?,?)"+
            				" WHEN matched then update set line_no = ?");
					do {
						String testResult = null;
						try {
							synchronized (q) {
								testResult = q.poll();
								if (null == testResult) break;
							}
							synchronized( this ) {
								//Long tc_id = null;
								File file = new File(testResult);
								Scanner scanner = new Scanner(file);
								Matcher matcher = null; 
								
					            while ( scanner.hasNextLine() ) {
					                String line = scanner.nextLine();
					                if (null == line || line.trim().isEmpty()) continue;
					                
					                matcher = func.matcher(line);
					                if ( matcher.find() ) {
				                		try {
				                			String src_file = matcher.group(3);
				                			Long lineNo = Long.parseLong(matcher.group(4));
				                			String func = matcher.group(2);
				                			src_file = src_file.replaceAll("(.*)(\\/.*\\/.*\\/.*)", "$2");
				                			if (!func.startsWith(".")) {
					                			setParameters(stmt2, src_file, func, src_file, lineNo, func, lineNo);
					                			int cnt = stmt2.executeUpdate();
					                			if (cnt < 1) {
					                				System.err.println("ERROR: Function inserte/update");
					                				System.err.println(line);
					                			}
					                			else {
					                				for (Emulator e : emuls) {
					                					e.Filter(line);
					                				}
					                				total += cnt;
					                			}
				                			}
				                			else {
				                				System.err.println(line);
				                			}
				                		}
				                		catch (SQLException e) {
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
				                		}
				                		continue;
					                }
					            }
					            
					            System.out.println(newLine);
							}
						}
						catch (FileNotFoundException e) {
				            e.printStackTrace();
				        }
						catch (Exception e) {
							e.printStackTrace();
						}
						busy = !q.isEmpty();
					} while (busy);
					stmt2.close();
					conn.close();
					System.out.println(String.format("DONE: total = %d\n", total));
					for (Emulator e : emuls) {
						System.out.println(String.format("\t %s", e.toString()));
    				}
				}
				catch (SQLException e1) {
					e1.printStackTrace();
				}
			}
		});
		t.setDaemon(true);
		t.start();
		}
	    return busy;
	}
}
