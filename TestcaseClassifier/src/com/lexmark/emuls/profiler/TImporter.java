package com.lexmark.emuls.profiler;
import java.io.File;
import java.io.FileNotFoundException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Scanner;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;

public class TImporter extends LinkedList<String> {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	static String newLine = System.getProperty("line.separator");
	private static TImporter queue = null;
	private static boolean busy = false;
	
	// invoking = ./pdls -e PDF ~/testcases/hello_lexmark.pdf
	private static String TESTCASE = "^invoking =\\s+.*";
	
	// [/bonus/scratch/tanch/pdls/app/main.c:631] => main
    private static String FUNC_HIT = "^\\[.*(\\/.*\\/.*\\/.*):(\\d+)\\]\\s+=>\\s+(.*)";
    
	private TImporter() throws Exception { 
		try {
			Class.forName("oracle.jdbc.driver.OracleDriver");
		} catch (ClassNotFoundException e) {
			System.out.println("Where is your Oracle JDBC Driver?");
			throw e;
		}
	}
	
	public static TImporter getInstance() throws Exception { 
		if (null == queue) {
			queue = new TImporter();
		}
		return queue;
	}
	
	private static Connection getDbConn() {
		Connection connection = null;
		try {
			connection = DriverManager.getConnection(
					"jdbc:oracle:thin:@localhost:1521:xe", "tc_profiler", "tc_profiler");
		} catch (SQLException e) {
			System.out.println("Connection Failed! Check output console");
			e.printStackTrace();
		}
		return connection;
	}
	
	public static boolean importFile(String filename) throws Exception { 
		final Queue<String> q = getInstance();
		synchronized (q) {
			q.add(filename);
		}
		if (!busy) {
			Thread t = new Thread(new Runnable() {
			public void run() {
				busy = true;
				Pattern re = Pattern.compile(FUNC_HIT);
				Pattern tc = Pattern.compile("^invoking =\\s+(.*)-e\\s+\\w{1,10}\\b(.*)");
				Connection conn = null;
				PreparedStatement stmt2 = null;
				PreparedStatement stmt3 = null;
				try {
					conn = getDbConn();
					stmt2 = conn.prepareStatement("MERGE INTO FUNC f using dual on (source_file=? AND func_name = ?)" +
            				" WHEN NOT matched then INSERT (SOURCE_FILE, LINE_NO, FUNC_NAME) values (?,?,?)"+
            				" WHEN matched then update set line_no = ?");
					
					stmt3 = conn.prepareStatement("MERGE INTO TESTCASE_FUNC_MAP using dual on (FID=? AND TLOC=?)" +
				            				" WHEN NOT matched then INSERT (FID, TLOC, SEQ) values (?,?,?)"+
				            				" WHEN matched then update set SEQ = ?");
					
					do {
						String testResult = null;
						try {
							synchronized (q) {
								testResult = q.poll();
							}
							long seqNo = 0; 
							String tloc = "";
							File file = new File(testResult);
							Scanner scanner = new Scanner(file);
				            while ( scanner.hasNextLine() ) {
				                String line = scanner.nextLine();
				                Matcher t = tc.matcher(line);
				                Matcher m = re.matcher(line);
			                	if ( t.find() ) {
				                	Path testcase = Paths.get(t.group(2));
				                	PreparedStatement tc_stmt = conn.prepareStatement("MERGE INTO TESTCASE using dual on (TLOC=?)" +
				            				" WHEN NOT matched then INSERT (TNAME, TLOC) values (?,?)"+
				            				" WHEN matched then update set TNAME = ?");
				                	String filename = testcase.getFileName().toString();
				                	tloc = testcase.toString();
				                	tc_stmt.setString(1, tloc);
				                	tc_stmt.setString(2, filename);
				                	tc_stmt.setString(3, tloc);
				                	tc_stmt.setString(4, filename);
				                	System.out.println("SOURCE: " + tloc);
				                	tc_stmt.executeUpdate();
				                	tc_stmt.close();
				            	}
				                else if ( m.find() ) {
				                	synchronized( this ) {
				                		try {
				                			String src_file = m.group(1);
				                			Long lineNo = Long.parseLong(m.group(2));
				                			String func = m.group(3);
				                			
				                			stmt2.setString(1, src_file);
				                			stmt2.setString(2, func);
				                			stmt2.setString(3, src_file);
				                			stmt2.setLong(4, lineNo);
				                			stmt2.setString(5, func);
				                			stmt2.setLong(6, lineNo);
				                			if (stmt2.executeUpdate() > 0) {
				                				long fid = 0;
					                			PreparedStatement pst = conn.prepareStatement("select FID FROM FUNC WHERE SOURCE_FILE=? AND FUNC_NAME=?");
					                			pst.setString(1, src_file);
					                			pst.setString(2, func);
					                			ResultSet rs = pst.executeQuery();
							                	if( rs.next()) {
							                		seqNo++;
							                		fid = rs.getLong(1);
						                			// conn.prepareStatement("MERGE INTO TESTCASE_FUNC_MAP using dual on (FID=? AND TLOC=?)" +
	//					            				" WHEN NOT matched then INSERT (FID, TLOC, SEQ) values (?,?,?)"+
	//					            				" WHEN matched then update set SEQ = ?");
						                			stmt3.setLong(1, fid);
						                			stmt3.setString(2, tloc);
						                			stmt3.setLong(3, fid);
						                			stmt3.setString(4, tloc);
						                			stmt3.setLong(5, seqNo);
						                			stmt3.setLong(6, seqNo);
						                			stmt3.addBatch();
							                	}
							                	rs.close();
							                	pst.close();
				                			}
				                		}
				                		catch (SQLException e) {
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
				                		}
				                	}
				                }
				            }
//				            stmt2.executeBatch();
				            stmt3.executeBatch();
				            
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
					stmt2.close();
					stmt3.close();
					conn.close();
					System.out.println("DONE!");
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
