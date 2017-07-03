package com.lexmark.emuls.profiler;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

abstract interface MyTask {
  public abstract boolean run(ResultSet rs) throws Exception;
}

abstract class MigrateTask extends DBConnection implements MyTask {
	private static final long serialVersionUID = 1L;
	String tableName;
	String taskSql;
	PreparedStatement t_stmt;
	Connection mySrc;
	Connection myDst;
	
	public MigrateTask (String fromSql, String toSql, Connection src, Connection dst) throws Exception {
		ResultSet rs = null;
		Statement stmt = null;
		
		tableName = "<NOT SET>";
		taskSql = toSql;
		mySrc = src;
		myDst = dst;
		
		try {
			t_stmt = myDst.prepareStatement(toSql);
			stmt = mySrc.createStatement();
			rs = stmt.executeQuery(fromSql);
			while (rs.next()) {
				try {
					if (!run (rs)) {
						break;
					}
				}
				catch (SQLException e) {
					e.printStackTrace();
				}
	        }
			rs.close(); rs = null;
			t_stmt.close();  t_stmt = null;
		} catch (SQLException e1) {
			e1.printStackTrace();
		}
		finally {
			if (null != rs) rs.close();
			if (null != t_stmt) t_stmt.close();
			if (null != stmt) stmt.close();
		}
	}
	
	protected ResultSet query(Connection conn, String sql, Object ... args) throws SQLException {
		ResultSet rs = null;
		try {
			PreparedStatement pst = conn.prepareStatement(sql);
			sql += ": " + setParameters(pst, args);
			
			rs = pst.executeQuery();
		}
		catch (Exception e) {
			throw new SQLException(sql,  e);
		}
    	return rs;
	}
	
	protected ResultSet querySource(String sql, Object ... args) throws SQLException {
    	return query (mySrc, sql, args);
	}
	
	protected ResultSet queryTarget(String sql, Object ... args) throws SQLException {
    	return query (myDst, sql, args);
	}
	
	protected Object queryTargetSingleResult(Class<?> type, String sql, Object ... args) throws SQLException {
		return sqlQuery (type, myDst, sql, args);
	}
}

public class MigrateDB extends DBConnection implements Runnable {
	private static final long serialVersionUID = 1L;
	
	protected MigrateDB() throws Exception {
		super();
	}

	@SuppressWarnings("serial")
	@Override
	public void run() {
		Connection newDb = null;
		Connection oldDb = null;
		try {
			oldDb = getDbConn("157.184.66.215", "tcprofiler", "tcprofiler");
			newDb = getDbConn("157.184.66.215", "tanch", "turtle");
			try {
				// copy all files from TESTCASE table.
//				new MigrateTask("SELECT * FROM TESTCASE",
//						"MERGE INTO TESTCASE USING dual ON (TGUID=?)" +
//		        				" WHEN NOT MATCHED THEN INSERT (TGUID,TNAME,TLOC,TTYPE,TSIZE,HIDDEN,UPDATE_DATE) values (?,?,?,?,?,?,?)"+
//		        				" WHEN MATCHED THEN UPDATE SET TSIZE=?,UPDATE_DATE=?",
//		        				oldDb, newDb) {
//
//					@Override
//					public boolean run(ResultSet rs) throws Exception {
//						String TGUID = rs.getString("TGUID");
//						Long TSIZE = rs.getLong("TSIZE");
//						int updateCount = 0;
//						System.out.println(setParameters(t_stmt, 
//		            					TGUID, 
//		            					TGUID,rs.getString("TNAME"),
//		            					rs.getString("TLOC"),
//		            					rs.getString("TTYPE"),
//		            					TSIZE,
//		            					rs.getString("HIDDEN"),
//		            					rs.getTimestamp("UPDATE_DATE"), 
//		            					TSIZE, new java.sql.Timestamp(System.currentTimeMillis())));
//						
//						updateCount = t_stmt.executeUpdate();
//						if (updateCount != 1) {
//							throw new Exception("Updated record count: " + updateCount);
//						}
//						return true;
//					}
//				};
				

//		new MigrateTask("SELECT * FROM FUNC",
//				"MERGE INTO FUNC f using dual on (SOURCE_FILE=? AND FUNC_NAME=?)" +
//    				" WHEN NOT matched then INSERT (FID,SOURCE_FILE,LINE_NO,FUNC_NAME,OLD_ID) values (?,?,?,?,?)"+
//    				" WHEN matched then update set LINE_NO=?,OLD_ID=?",
//				oldDb, newDb) {
//
//			@Override
//			public boolean run(ResultSet rs) throws Exception {
//				String SOURCE_FILE = rs.getString("SOURCE_FILE");
//				String FUNC_NAME = rs.getString("FUNC_NAME");
//				Long LINE_NO = rs.getLong("LINE_NO");
//				Long OLD_ID = rs.getLong("FID");
//				
//				Long FID = Integer.toUnsignedLong(SOURCE_FILE.hashCode() + FUNC_NAME.hashCode());
//				int updateCount = 0;
//				System.out.println(setParameters(t_stmt, 
//						SOURCE_FILE, FUNC_NAME,
//						FID,SOURCE_FILE,LINE_NO,FUNC_NAME,OLD_ID,
//						LINE_NO,OLD_ID));
//				
//				updateCount = t_stmt.executeUpdate();
//				if (updateCount != 1) {
//					throw new Exception("Updated record count: " + updateCount);
//				}
//				return true;
//			}
//		};
				
				// d5e9b41d1b02f2be655070ecc35135a4
				new MigrateTask("SELECT * FROM TESTCASE_FUNC WHERE PID = 5",
						"MERGE INTO TESTCASE_FUNC USING dual on (TGUID=? AND FID=? AND PID=?)" +
	            				" WHEN NOT MATCHED THEN INSERT (TGUID,FID,PID,SEQ) values (?,?,?,?)"+
	            				" WHEN MATCHED THEN UPDATE SET SEQ = ?",
		        				oldDb, newDb) {

					@Override
					public boolean run(ResultSet rs) throws Exception {
						int updateCount = 0;
						String TGUID = rs.getString("TGUID");
						Long FID = (Long)queryTargetSingleResult(Long.class, "SELECT FID FROM FUNC WHERE OLD_ID=?",  rs.getLong("FID"));
						Long PID = 1L; //rs.getLong("PID");
						Long SEQ = rs.getLong("SEQ");
						
						System.out.println(setParameters(t_stmt, 
										TGUID, FID, PID,
										TGUID, FID, PID, SEQ, 
										SEQ));
						
						updateCount = t_stmt.executeUpdate();
						if (updateCount != 1) {
							throw new Exception("Updated record count: " + updateCount);
						}
						return true;
					}
				};
			}
			catch (Exception e) {
				e.printStackTrace();
			}
			finally {
				if (null != oldDb) {
					oldDb.close();
					oldDb = null;
				}
				if (null != newDb) {
					newDb.close();
					newDb = null;
				}
				System.out.println("DONE!");
			}
		}
		catch (Exception e) {
			System.err.println("FATAL ERROR!");
			System.exit(1);
		}
	}
	
	public static void main(String[] args) {
		try {
			System.out.println( "nvStart".hashCode() );
			new MigrateDB().run();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
}
