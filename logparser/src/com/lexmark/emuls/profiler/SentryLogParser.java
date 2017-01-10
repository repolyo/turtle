package com.lexmark.emuls.profiler;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.sql.Connection;
import java.sql.Date;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Scanner;
import java.util.StringTokenizer;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.omg.PortableInterceptor.ORBInitInfoPackage.DuplicateName;

public class SentryLogParser extends LinkedList<String> {
	/**
	 * 
	 */
	public static final String LOG_EXT = ".out";
	private static final long serialVersionUID = 1L;
	static String newLine = System.getProperty("line.separator");
	private static SentryLogParser queue = null;
	private static boolean busy = false;
	
	public static String dbUser = "tcprofiler";
	public static String dbPasswd = "********";
	public static String dbHost = System.getenv("TURTLE_DB");//"157.184.66.215";
	
	// 11/27/15 20:03:19
	static DateFormat DATE_FORMATTER = new SimpleDateFormat("MM/dd/yy HH:mm:ss");
	private static String PERSONA_HIT = "^#\\s+persona\\s*:\\s+(.*)$";
	private static String RESOLUTION = "^#\\s+resolution\\s*:\\s+(.*)$";
	
	// # location : /m/tcases/futures/next/wip/
	private static String LOCATION = "^#\\s+location\\s*:\\s+(.*)$";
	
	// pdf/viewer_prefs/p4_3up.pdf : f9d99306,788ee140
	private static String TESTCASE = "^(?!#)(.*)\\s*:\\s+(.*)$";
	
	private SentryLogParser() throws Exception { 
		try {
			Class.forName("oracle.jdbc.driver.OracleDriver");
		} catch (ClassNotFoundException e) {
			System.out.println("Where is your Oracle JDBC Driver?");
			throw e;
		}
	}
	
	public static SentryLogParser getInstance() throws Exception { 
		if (null == queue) {
			queue = new SentryLogParser();
		}
		return queue;
	}
	
	private static Connection getDbConn() {
		Connection connection = null; 
		try {
			connection = DriverManager.getConnection(
					"jdbc:oracle:thin:@"+dbHost+":1521:xe", dbUser, dbPasswd);
			System.out.format(String.format("Connected to: %s@%s DB...", dbHost, dbUser));
		} catch (SQLException e) {
			System.out.println("Connection Failed! Check output console");
			e.printStackTrace();
		}
		return connection;
	}
	
	private static String setParameters(PreparedStatement stmt, Object ... args) throws SQLException {
		int i = 1;
		for (Object param : args) {
			if (param instanceof String) {
				stmt.setString(i, param.toString());		
			}
			else if (param instanceof Number) {
				stmt.setLong(i, ((Number)param).longValue() );		
			}
			else if (param instanceof Timestamp) {
				stmt.setTimestamp(i, (Timestamp)param);
			}
			else if (param instanceof Date) {
				stmt.setDate(i, (Date)param);
			}
			i++;
		}
		return "";
	}
	
	private static boolean insert(Connection conn, String sql, Object ... args) throws SQLException {
		boolean ok = false;
		try {
			PreparedStatement pst = conn.prepareStatement(sql);
			setParameters(pst, args);
			if ( pst.executeUpdate() > 0 ) {
				ok = true;
	    	}
			pst.close();
		}
		catch (Exception e) {
			throw new SQLException(sql + Arrays.toString(args) ,  e);
		}
		return ok;
	}
	
	private static int update(Connection conn, String sql, Object ... args) throws SQLException {
		int updated = -1;
		try {
			PreparedStatement pst = conn.prepareStatement(sql);
			setParameters(pst, args);
			updated = pst.executeUpdate();
			pst.close();
		}
		catch (Exception e) {
			throw new SQLException(sql + Arrays.toString(args) ,  e);
		}
		return updated;
	}
	
	private static Object sqlQuery(Class<?> type, Connection conn, String sql, Object ... args) throws SQLException {
		Object ret = null;
		try {
			PreparedStatement pst = conn.prepareStatement(sql);
			setParameters(pst, args);
			ResultSet rs = pst.executeQuery();
	    	if( rs.next()) {
	    		if (Long.class == type) {
	    			ret = rs.getLong(1);
	    		}
	    		else if (String.class == type) {
	    			ret = rs.getString(1);
	    		}
	    	}
	    	rs.close();
	    	pst.close();
		}
		catch (Exception e) {
			throw new SQLException(sql,  e);
		}
    	return ret;
	}
	
	public static boolean importFile(String filename) throws Exception { 
		final Queue<String> q = getInstance();
		synchronized (q) {
			if (q.contains(filename)) {
				throw new DuplicateName("already is processing: " + filename);
			}
			q.add(filename);
		}
		if (!busy) {
			Thread t = new Thread(new Runnable() {
			public void run() {
				busy = true;
				Pattern per = Pattern.compile(PERSONA_HIT);
				Pattern res = Pattern.compile(RESOLUTION);
				Pattern tc = Pattern.compile(TESTCASE);
				Pattern loc = Pattern.compile(LOCATION);
				
				Connection conn = null;
				PreparedStatement statement = null;
				try {
					String sql = "";
					conn = getDbConn();
					statement = conn.prepareStatement(
							"MERGE INTO TESTCASE_CHECKSUM using dual on (TGUID=? AND PID=? AND PAGE_NO=?)" +
							" WHEN NOT matched THEN " + 
							" INSERT (TGUID,PID,PAGE_NO,CHECKSUM,DURATION) VALUES (?,?,?,?,NUMTODSINTERVAL(2.060018, 'SECOND'))"+
							" WHEN matched then UPDATE SET CHECKSUM=?");
					do {
						String testResult = null;
						try {
							synchronized (q) {
								testResult = q.poll();
								if (null == testResult) break;
							}
							synchronized( this ) {
								boolean error = false;
								Matcher matcher = null; 
								File file = new File(testResult);
								PrintWriter log = new PrintWriter(new FileWriter(testResult+LOG_EXT));
								Scanner scanner = new Scanner(file);
								long pid = 0; // default 
								
								String persona = "";
								String resolution = "";
								String tguid = "";
								String location = "";
								
					            while ( scanner.hasNextLine() ) {
					            	error = false;
					                String line = scanner.nextLine();
					                
					                if (0 == pid && !persona.isEmpty() && !resolution.isEmpty()) {
	                					Long id = (Long)sqlQuery(Long.class, conn, "select PID FROM PLATFORM WHERE PERSONA=? AND RESOLUTION=?", persona, resolution);
					                	if( null != id) {
					                		pid = id;
					                	}
	                				}
					                
					                if (persona.isEmpty() && null != (matcher = per.matcher(line)) && matcher.find() ) {
					                	persona = matcher.group(1).trim();
		                				continue;
					                }
					                else if ( resolution.isEmpty() && null != (matcher = res.matcher(line)) && matcher.find() ) {
					                	String dpi = matcher.group(1).trim();
		                				if (!dpi.isEmpty()) {
		                					resolution = dpi;
		                				}
		                				continue;
					                }
					                else if (location.isEmpty() && null != (matcher = loc.matcher(line)) && matcher.find() ) {
					                	location = matcher.group(1).trim();
					                }
					                else if (0 != pid && null != (matcher = tc.matcher(line)) && matcher.find() ) {
					                	int page = 1;
					                	String testcase = matcher.group(1);
				                		String checksums = matcher.group(2);
					                	String filename = location + testcase;
					                	StringTokenizer cs = new StringTokenizer(checksums,",");
					                	
					                	tguid = (String)sqlQuery(String.class, conn, "select TGUID FROM TESTCASE WHERE TLOC=?", filename.trim());
					                	if (null == tguid) {
					                		continue;
					                	}
					                	
					                	while(cs.hasMoreTokens()) {
					                		String c = cs.nextToken().trim();
					                		sql = String.format("MERGE INTO TESTCASE_CHECKSUM using dual on (TGUID='%s' AND PID=%d AND PAGE_NO=%d) WHEN NOT matched then " +
					                				"INSERT (TGUID,PID,PAGE_NO,CHECKSUM,DURATION) values ('%s',%d,%d,'%s',NUMTODSINTERVAL(2.060018, 'SECOND'))"+
					    							" WHEN matched then UPDATE SET CHECKSUM='%s';",
					    							tguid, pid, page, tguid, pid, page, c, c);

					                		sql += "\nlocation=" + location;
					                		sql += "\ntestcase=" + testcase;
					                		sql += "\nchecksums=" + checksums;
					                		sql += "\nfilename=" + filename;
					                		
						                	setParameters(statement, tguid, pid, page, tguid, pid, page, c, c);
						                	statement.addBatch();
						                	page++;
					                	}
					                	
					                	// delete previous records to replace them!
					                	log.println(String.format("%d == DELETE FROM TESTCASE_CHECKSUM WHERE TGUID='%s' AND PID=%d AND PAGE_NO >= %d", 
					                			update(conn, "DELETE FROM TESTCASE_CHECKSUM WHERE TGUID=? AND PID=? AND PAGE_NO >= ?", tguid, pid, page),
					                			tguid, pid, page));
					                }
					            }
					            statement.executeBatch();
					            scanner.close();
					            if ( error ) { // for review: backup file on error!
					            	Files.move(Paths.get(file.getPath()), 
					            		Paths.get(file.getParent() + "/tmp/"+ file.getName()), 
					            		StandardCopyOption.REPLACE_EXISTING);
					            }
					            else if ( !file.delete() ) {
					            	new FileNotFoundException("Unable to delete: " + testResult).printStackTrace();
					            };
					            log.close();
					            System.out.println(newLine);
		//			            Thread.yield();
							}
						}
						catch (FileNotFoundException e) {
				            e.printStackTrace();
				        } catch (Exception e) {
							e = new SQLException(sql, e);
							e.printStackTrace();
						}
						busy = !q.isEmpty();
					} while (busy);
					statement.close();
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