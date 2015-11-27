package com.lexmark.emuls.profiler;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.nio.file.CopyOption;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Scanner;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.omg.PortableInterceptor.ORBInitInfoPackage.DuplicateName;

import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.Connection;
import java.sql.Date;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;

public class TImporter extends LinkedList<String> {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	static String newLine = System.getProperty("line.separator");
	private static TImporter queue = null;
	private static boolean busy = false;
	
	// 11/27/15 20:03:19
	static DateFormat DATE_FORMATTER = new SimpleDateFormat("MM/dd/yy HH:mm:ss");
		
	// invoking = ./pdls -e PDF ~/testcases/hello_lexmark.pdf
	private static String TESTCASE = "^invoking =\\s+(.*)(-e\\s+\\w{1,10}\\b|\\s+)(.*)";
	
	// [/bonus/scratch/tanch/pdls/app/main.c:631] => main
    private static String FUNC_HIT = "^\\[.*(\\/.*\\/.*\\/.*):(\\d+)\\]\\s+=>\\s+(.*)";
    
    // [tag with: ASCII85Decode]
    private static String TAG_HIT = "^\\[tag with:\\s+(.*)\\]";
    
    // [@startTime: 1447680618]
    private static String START_TIME = "^\\[@startTime:\\s+(\\d{2}\\/\\d{2}\\/\\d{2}\\s\\d{2}:\\d{2}:\\d{2})\\]";
    
    // [@endTime: 1447680618]
    private static String END_TIME = "^\\[@endTime:\\s+(\\d{2}\\/\\d{2}\\/\\d{2}\\s\\d{2}:\\d{2}:\\d{2})\\]";
    
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
	
	private static void setParameters(PreparedStatement stmt, Object ... args) throws SQLException {
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
	}
	
	private static boolean insert(Connection conn, String sql, Object ... args) throws SQLException {
		boolean ok = false;
		PreparedStatement pst = conn.prepareStatement(sql);
		setParameters(pst, args);
		if ( pst.executeUpdate() > 0 ) {
			ok = true;
    	}
		pst.close();
		return ok;
	}
	
	private static Object sqlQuery(Class<?> type, Connection conn, String sql, Object ... args) throws SQLException {
		Object ret = null;
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
				Pattern re = Pattern.compile(FUNC_HIT);
				Pattern tc = Pattern.compile(TESTCASE);
				Pattern tg = Pattern.compile(TAG_HIT);
				Pattern st = Pattern.compile(START_TIME);
				Pattern et = Pattern.compile(END_TIME);
				
				Connection conn = null;
				PreparedStatement stmt2 = null;
				PreparedStatement stmt3 = null;
				PreparedStatement stmt4 = null;
				PreparedStatement stmt5 = null;
				try {
					conn = getDbConn();
					stmt2 = conn.prepareStatement("MERGE INTO FUNC f using dual on (source_file=? AND func_name = ?)" +
            				" WHEN NOT matched then INSERT (SOURCE_FILE, LINE_NO, FUNC_NAME) values (?,?,?)"+
            				" WHEN matched then update set line_no = ?");
					
					stmt3 = conn.prepareStatement("MERGE INTO TESTCASE_FUNC using dual on (FID=? AND TID=?)" +
				            				" WHEN NOT matched then INSERT (FID, TID, SEQ) values (?,?,?)"+
				            				" WHEN matched then update set SEQ = ?");
					
					stmt4 = conn.prepareStatement("MERGE INTO TAGS using dual on (TAG_NAME=?)" +
            				" WHEN NOT matched then INSERT (TAG_NAME) values (?)"+
            				" WHEN matched then update set CREATE_DATE = ?");
					
					stmt5 = conn.prepareStatement("MERGE INTO TESTCASE_TAGS using dual on (TAG_ID=? AND TID=?)" +
            				" WHEN NOT matched then INSERT (TAG_ID,TID) values (?,?)"+
            				" WHEN matched then update set CREATE_DATE = ?");
					
					do {
						String testResult = null;
						try {
							synchronized (q) {
								testResult = q.poll();
								if (null == testResult) break;
							}
							synchronized( this ) {
								long seqNo = 0; 
								String tloc = "";
								Long tc_id = null;
								Matcher matcher = null; 
								Timestamp startTime = null;
								Timestamp endTime = null;
								File file = new File(testResult);
								Scanner scanner = new Scanner(file);
					            while ( scanner.hasNextLine() ) {
					                String line = scanner.nextLine();
					                matcher = tc.matcher(line);
					                if ( matcher.find() ) {
				                		String testcase = matcher.group(3);
					                	String filename = testcase.substring(testcase.lastIndexOf('/')+1);
					                	PreparedStatement tc_stmt = conn.prepareStatement("MERGE INTO TESTCASE using dual on (TLOC=?)" +
					            				" WHEN NOT matched then INSERT (TNAME, TLOC) values (?,?)"+
					            				" WHEN matched then update set TNAME = ?");
					                	tloc = testcase.toString();
					                	setParameters(tc_stmt, tloc, filename, tloc, filename);
					                	System.out.println("SOURCE: " + tloc);
					                	if ( tc_stmt.executeUpdate() > 0 ) {
					                		tc_id = (Long)sqlQuery(Long.class, conn, "SELECT TID from TESTCASE where TLOC=?", tloc);
					                	}
					                	tc_stmt.close();
					                	continue;
					            	}
					                
					                matcher = re.matcher(line);
					                if ( matcher.find() ) {
				                		try {
				                			String src_file = matcher.group(1);
				                			Long lineNo = Long.parseLong(matcher.group(2));
				                			String func = matcher.group(3);
				                			setParameters(stmt2, src_file, func, src_file, lineNo, func, lineNo);
				                			if (stmt2.executeUpdate() > 0) {
				                				Long fid = (Long)sqlQuery(Long.class, conn, "select FID FROM FUNC WHERE SOURCE_FILE=? AND FUNC_NAME=?", src_file, func);
							                	if( null != fid && null != tc_id) {
							                		seqNo++;
							                		setParameters(stmt3, fid, tc_id, fid, tc_id, seqNo, seqNo);
						                			stmt3.addBatch();
							                	}
				                			}
				                		}
				                		catch (SQLException e) {
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
				                		}
				                		continue;
					                }
					                
					                matcher = tg.matcher(line);
					                if ( matcher.find() ) {
			                			try {
			                				String tag = matcher.group(1);
			                				setParameters(stmt4, tag, tag, new Date(System.currentTimeMillis()));
				                			if (stmt4.executeUpdate() > 0) {
				                				Long tag_id = (Long)sqlQuery(Long.class, conn, "select TID FROM TAGS WHERE TAG_NAME=?", tag);
				                				setParameters(stmt5, tag_id, tc_id, tag_id, tc_id, new Date(System.currentTimeMillis()));
				                				stmt5.addBatch();
				                			}
			                			}
				                		catch (SQLException e) {
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
			                			}
			                			continue;
				                	}
				                	
					                matcher = st.matcher(line);
				                	if ( matcher.find() ) {
				                		try {
											startTime = new java.sql.Timestamp(DATE_FORMATTER.parse(matcher.group(1)).getTime());
										} catch (ParseException e) {
											// TODO Auto-generated catch block
											e.printStackTrace();
										}
				                		continue;
				                	}
				                	matcher = et.matcher(line);
				                	if (null != startTime && matcher.find() ) {
				                		try {
				                			endTime = new java.sql.Timestamp(DATE_FORMATTER.parse(matcher.group(1)).getTime());
											insert(conn, "INSERT INTO TESTCASE_RUN (TID, START_TIME, END_TIME) VALUES (?,?,?)", 
					                				tc_id, startTime, endTime);
											
										} catch (ParseException e) {
											// TODO Auto-generated catch block
											e.printStackTrace();
										}
				                		continue;
				                	}
					            }
					            scanner.close();
					            
					            Files.move(Paths.get(file.getPath()), 
					            		Paths.get(file.getParent() + "/tmp/"+ file.getName()), 
					            		StandardCopyOption.REPLACE_EXISTING);
					            
//					            if ( !file.delete() ) {
//					            	throw new FileNotFoundException("Unable to delete: " + testResult);
//					            };
	//				            stmt2.executeBatch();
					            stmt3.executeBatch();
					            stmt5.executeBatch();
					            
					            System.out.println(newLine);
		//			            Thread.yield();
							}
						}
						catch (FileNotFoundException e) {
				            e.printStackTrace();
				        } catch (IOException e) {
							// TODO Auto-generated catch block
							e.printStackTrace();
						}
						busy = !q.isEmpty();
					} while (busy);
					stmt2.close();
					stmt3.close();
					stmt4.close();
					stmt5.close();
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
