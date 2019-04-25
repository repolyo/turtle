package com.lexmark.emuls.profiler;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.sql.BatchUpdateException;
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
import java.util.Queue;
import java.util.Scanner;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.omg.PortableInterceptor.ORBInitInfoPackage.DuplicateName;

public class FunctionMapper extends DBConnection {
	
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	private static FunctionMapper queue = null;
	private static boolean busy = false;
	
	public static boolean checksum_update = false;
	
	// 11/27/15 20:03:19
	static DateFormat DATE_FORMATTER = new SimpleDateFormat("MM/dd/yy HH:mm:ss");
		
	private static String PERSONA_HIT = "^\\[@persona:\\s+(.*)\\]";
	
	// invoking = ./pdls -e PDF ~/testcases/hello_lexmark.pdf
	//> ./pdls -s ErrorFW900.00-P4.PCL5.pcl 
	private static String TESTCASE = "^> ./pdls\\s+-s\\s+(.*)";
	
	// [/usr/src/debug/pdls/0.0+gitAUTOINC+13f3a51bac-r0/git/cfs/cfslinux_cache.c:330] => cfs_linux_cache_build_device_cache
	// /bonus/scratch/tanch/build-bundle/poky/sim-color/tmp/work/i586-poky-linux/pdls/0.0+gitAUTOINC+13f3a51bac-r0/build/pdf/pdflex.c:396
    private static String FUNC_HIT = "^\\[(.*)\\/(.*)\\/(.*)git(.*)(git|build).*\\/(.*):(\\d+)\\]\\s+=>\\s+(.*)";
    
    // Page 1 checksum is: e66f977c
    private static String CHECKSUM_HIT = "^Page (\\d+)\\s+checksum is:\\s+(.*)$";
    
    // [tag with: ASCII85Decode]
    private static String TAG_HIT = "^\\[tag with:\\s+(.*)\\]";
    
    // [@startTime: 1447680618]
    private static String START_TIME = "^\\[@startTime:\\s+(\\d{2}\\/\\d{2}\\/\\d{2}\\s\\d{2}:\\d{2}:\\d{2})\\]";
    
    // [@endTime: 1447680618]
    private static String END_TIME = "^\\[@endTime:\\s+(\\d{2}\\/\\d{2}\\/\\d{2}\\s\\d{2}:\\d{2}:\\d{2})\\]";
    
    private static String PJL_COMMAND = ".*@PJL(.*)$";
    
    private static String RESOLUTION = "^\\[@resolution:\\s+(.*)\\]";
    
    private static String GUID_REGEX = "^\\[@guid:\\s+([0-9a-z]+)*(.*)\\]";
    
	protected FunctionMapper() throws Exception {
		super();
		// TODO Auto-generated constructor stub
	}
	
	public static FunctionMapper getInstance() throws Exception { 
		if (null == queue) {
			queue = new FunctionMapper();
		}
		return queue;
	}
	
	public boolean importFile(String filename) throws Exception { 
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
				Pattern re = Pattern.compile(FUNC_HIT);
				Pattern tc = Pattern.compile(TESTCASE);
				Pattern tg = Pattern.compile(TAG_HIT);
				Pattern st = Pattern.compile(START_TIME);
				Pattern et = Pattern.compile(END_TIME);
				
				Pattern cs = Pattern.compile(CHECKSUM_HIT);
				Pattern pjl = Pattern.compile(PJL_COMMAND);
				Pattern res = Pattern.compile(RESOLUTION);
				Pattern guid = Pattern.compile(GUID_REGEX);
				
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
					
					stmt3 = conn.prepareStatement("MERGE INTO TESTCASE_FUNC using dual on (PID=? AND FID=? AND TGUID=?)" +
				            				" WHEN NOT matched then INSERT (PID, FID, TGUID, SEQ) values (?,?,?,?)"+
				            				" WHEN matched then update set SEQ = ?");
					
					stmt4 = conn.prepareStatement("MERGE INTO TAGS using dual on (TAG_NAME=?)" +
            				" WHEN NOT matched then INSERT (TAG_NAME,TAG_DESCR) values (?,?)"+
            				" WHEN matched then update set UPDATE_DATE = ?");
					
					stmt5 = conn.prepareStatement("MERGE INTO TESTCASE_TAGS using dual on (TAG_ID=? AND TGUID=?)" +
            				" WHEN NOT matched then INSERT (TGUID,TAG_ID) values (?,?)"+
            				" WHEN matched then update set UPDATE_DATE = ?");
					
					do {
						String testResult = null;
						try {
							synchronized (q) {
								testResult = q.poll();
								if (null == testResult) break;
							}
							synchronized( this ) {
								boolean error = false;
								boolean tc_inserted = false;
								long seqNo = 0; 
								String tloc = "";
								//Long tc_id = null;
								Matcher matcher = null; 
								Timestamp startTime = null;
								Timestamp endTime = null;
								File file = new File(testResult);
								Scanner scanner = new Scanner(file);
								long pid = 0; // default 
								
								String persona = "";
								String resolution = "";
								String tguid = "";
				                
					            while ( scanner.hasNextLine() ) {
					            	error = false;
					                String line = scanner.nextLine();
					                if (null == line || line.trim().isEmpty()) continue;
					                
					                if (0 == pid) {
    					                matcher = per.matcher(line);
    					                if ( matcher.find() ) {
    					                	persona = matcher.group(1).trim();
    		                				continue;
    					                }
    					                
    					                matcher = res.matcher(line);
    					                if ( matcher.find() ) {
    					                	String dpi = matcher.group(1).trim();
    		                				if (!dpi.isEmpty()) {
    		                					resolution = dpi;
    		                				}
    		                				continue;
    					                }
    					                
    					                if (0 == pid && !persona.isEmpty() && !resolution.isEmpty()) {
    	                					Long id = (Long)sqlQuery(Long.class, conn, "select PID FROM PLATFORM WHERE PERSONA=? AND RESOLUTION=?", persona, resolution);
    					                	if( null != id) {
    					                		pid = id;
    					                	}
    					                	else {
    					                		System.err.println("ERROR: Could not get a platform where this testcase war ran.");
    					                		break; // bail out! invalid testcase}
    					                	}
    	                				}
    					                
    					                continue;
					                }
					                
                                    matcher = guid.matcher(line);
                                    if ( matcher.find() ) {
                                        tguid = matcher.group(1).trim();
                                        continue;
                                    }
					                
					                if (tguid.isEmpty()) {
    					                // "^invoking =\\s+(.*)-e\\s+(\\w{1,10}\\b|\\s+)(.*)";
    					                matcher = tc.matcher(line);
    					                if ( matcher.find() ) {
    					                	String size = "0";
    				                		String testcase = matcher.group(1);
    				                		String type = "-";
    					                	String filename = testcase.substring(testcase.lastIndexOf('/')+1);
    					                	String sql = "MERGE INTO TESTCASE using dual on (TLOC=?)" +
    					            				" WHEN NOT matched then INSERT (TNAME,TLOC,TTYPE,TSIZE) values (?,?,?,?)"+
    					            				" WHEN matched then update set TNAME=?,TTYPE=?,TSIZE=?,UPDATE_DATE=?";
    					                	
    					                	PreparedStatement tc_stmt = conn.prepareStatement(sql);
    					                	tloc = testcase.toString().trim();
    					                	System.out.println(sql + " " + 
    					                			setParameters(tc_stmt, tloc, filename, tloc, type, size, 
    							                			filename, type, size, new java.sql.Timestamp(System.currentTimeMillis())));
    					                	
    					                	System.out.println(String.format("\nSOURCE(%s): %s\n", type, tloc));
    					                	tc_inserted = tc_stmt.executeUpdate() == 1;
    					                	tc_stmt.close();
    					                	
    					                	tguid = (String)sqlQuery(String.class, conn, "select TGUID FROM TESTCASE WHERE TLOC=?", tloc);
    					                	
    					                	// clean up old records! new records will have runtime id!!!
    					                	System.out.println(String.format(
    					                			"DELETE FROM TESTCASE_FUNC WHERE TGUID=? AND (PID=?)", tguid, pid));
    					                	update(conn, "DELETE FROM TESTCASE_FUNC WHERE TGUID=? AND (PID=?)", tguid, pid);
    					                	continue;
    					            	}
    					                
    					                if ( !tc_inserted ) continue;
    					                
    					                continue;
					                }
					                
					                matcher = cs.matcher(line);
					                if ( matcher.find() ) {
					                	if ( checksum_update ) {
						                	Long page_no = Long.parseLong(matcher.group(1));
						                	String checksum = matcher.group(2);
						                	System.out.println(line + " : page" + page_no + ", checksum: " + checksum);
						                	PreparedStatement cs_stmt = conn.prepareStatement("MERGE INTO TESTCASE_CHECKSUM using dual on (TGUID=? AND PAGE_NO=? AND PID=?)" +
						            				" WHEN NOT matched then INSERT (TGUID,PAGE_NO,CHECKSUM,PID) values (?,?,?,?)"+
						            				" WHEN matched then update set CHECKSUM=?");
						                	setParameters(cs_stmt, tguid, page_no, pid, tguid, page_no, checksum, pid, checksum);
						                	cs_stmt.executeUpdate();
						                	cs_stmt.close();
					                	}
					                	continue;
					                }
					                
					                matcher = re.matcher(line);
					                if ( matcher.find() ) {
				                		try {
				            				String unit = matcher.group(2);
				            				String src_file = String.format("%s@%s", unit, matcher.group(6));
				                			Long lineNo = Long.parseLong(matcher.group(7));
				                			String func = matcher.group(8);
				                			
//				                			System.out.println("unit: " + unit);
//				                			System.out.println("src_file: " + src_file);
//				                			System.out.println("lineNo: " + lineNo);
//				                			System.out.println("func: " + func);
				                			
				                			setParameters(stmt2, src_file, func, src_file, lineNo, func, lineNo);
				                			if (stmt2.executeUpdate() > 0) {
				                				Long fid = (Long)sqlQuery(Long.class, conn, "select FID FROM FUNC WHERE SOURCE_FILE=? AND FUNC_NAME=?", src_file, func);
							                	if( null != fid) {
							                		seqNo++;
							                		setParameters(stmt3, pid, fid, tguid, pid, fid, tguid, seqNo, seqNo);
						                			stmt3.addBatch();
							                	}
				                			}
				                		}
				                		catch (SQLException e) {
				                			error = true;
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
				                		}
				                		continue;
					                }
					                
					                matcher = tg.matcher(line);
					                if ( matcher.find() ) {
			                			try {
			                				String tag = matcher.group(1).trim();
			                				if (!tag.isEmpty()) {
				                				setParameters(stmt4, tag, tag, tag, new Date(System.currentTimeMillis()));
					                			if (stmt4.executeUpdate() > 0) {
					                				Long tag_id = (Long)sqlQuery(Long.class, conn, "select TID FROM TAGS WHERE TAG_NAME=?", tag);
					                				/* "MERGE INTO TESTCASE_TAGS using dual on (TAG_ID=? AND TGUID=?)" +
					                        				" WHEN NOT matched then INSERT (TGUID,TAG_ID) values (?,?)"+
					                        				" WHEN matched then update set UPDATE_DATE = ?"); */
					                				setParameters(stmt5, tag_id, tguid, tguid, tag_id, new Date(System.currentTimeMillis()));
					                				stmt5.addBatch();
					                			}
			                				}
			                			}
				                		catch (SQLException e) {
				                			error = true;
				                			System.out.println("Connection Failed! Check output console");
				                			e.printStackTrace();
			                			}
			                			continue;
				                	}
					                
					                matcher = pjl.matcher(line);
					                if ( matcher.find() ) {
			                			try {
			                				String pjlDesc = matcher.group(1).trim();
			                				if (!pjlDesc.isEmpty()) {
			                					String pjlCmd = "@PJL " + pjlDesc;
				                				setParameters(stmt4, pjlCmd, pjlCmd, pjlDesc, new Date(System.currentTimeMillis()));
					                			if (stmt4.executeUpdate() > 0) {
					                				Long tag_id = (Long)sqlQuery(Long.class, conn, "select TID FROM TAGS WHERE TAG_NAME=?", pjlCmd);
					                				/* "MERGE INTO TESTCASE_TAGS using dual on (TAG_ID=? AND TGUID=?)" +
					                        				" WHEN NOT matched then INSERT (TGUID,TAG_ID) values (?,?)"+
					                        				" WHEN matched then update set UPDATE_DATE = ?"); */
					                				setParameters(stmt5, tag_id, tguid, tguid, tag_id, new Date(System.currentTimeMillis()));
					                				stmt5.addBatch();
					                			}
			                				}
			                			}
				                		catch (SQLException e) {
				                			error = true;
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
											error = true;
											e.printStackTrace();
										}
				                		continue;
				                	}
				                	matcher = et.matcher(line);
				                	if (tc_inserted && null != startTime && matcher.find() ) {
				                		try {
				                			endTime = new java.sql.Timestamp(DATE_FORMATTER.parse(matcher.group(1)).getTime());
											insert(conn, "INSERT INTO TESTCASE_RUN (TGUID, PID, START_TIME, END_TIME) VALUES (?,?,?,?)", 
													tguid, pid, startTime, endTime);
											tc_inserted = false;
											System.out.println("INSERT INTO TESTCASE_RUN: " + testResult);
										} catch (ParseException e) {
											error = true;
											e.printStackTrace();
										}
				                		continue;
				                	}
					            }
					            scanner.close();
					            if ( error ) { // for review: backup file on error!
					            	Files.move(Paths.get(file.getPath()), 
					            		Paths.get(file.getParent() + "/tmp/"+ file.getName()), 
					            		StandardCopyOption.REPLACE_EXISTING);
					            }
					            else if ( !file.delete() ) {
					            	new FileNotFoundException("Unable to delete: " + testResult).printStackTrace();
					            };
	//				            stmt2.executeBatch();
					            stmt3.executeBatch();
					            stmt5.executeBatch();
					            
					            System.out.println(newLine);
		//			            Thread.yield();
							}
						}
						catch (BatchUpdateException e) {
							e.printStackTrace();
						}
						catch (FileNotFoundException e) {
				            e.printStackTrace();
				        }
						catch (IOException e) {
							e.printStackTrace();
						}
						catch (Exception e) {
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

/*	
	public static void main(String[] args) {
		Matcher matcher = null; 
		String [] samples = {
			"[/usr/src/debug/pdls/0.0+gitAUTOINC+13f3a51bac-r0/git/cfs/cfslinux_cache.c:330] => cfs_linux_cache_build_device_cache",
			"[/bonus/scratch/tanch/build-bundle/poky/sim-color/tmp/work/i586-poky-linux/pdls/0.0+gitAUTOINC+13f3a51bac-r0/build/pdf/pdflex.c:396] => xxx",
			"[/usr/src/debug/ufst/0.0+gitAUTOINC+e5ef7b9603-r0/git/tt_if.c:9230] => ufst",
			"[/usr/src/debug/graphen/0.0+gitAUTOINC+ab1da16696-r0/git/blockdevicepage/mulblit.c:6027] => xiTile"
		};
		for (int i=0; i < samples.length; i++) {
			String line = samples[i];
			matcher = Pattern.compile("^\\[(.*)\\/(.*)\\/(.*)gitAUTOINC(.*)(git|build)\\/(.*):(\\d+)\\]\\s+=>\\s+(.*)").matcher(line);
			if ( matcher.find() ) {
				String unit = matcher.group(2);
				String src_file = String.format("%s@%s", unit, matcher.group(6));
    			Long lineNo = Long.parseLong(matcher.group(7));
    			String func = matcher.group(8);
    			
    			System.out.println("unit: " + unit);
    			System.out.println("src_file: " + src_file);
    			System.out.println("lineNo: " + lineNo);
    			System.out.println("func: " + func);
			}
        }
	}
*/

}
