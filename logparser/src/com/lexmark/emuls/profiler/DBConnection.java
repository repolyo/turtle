package com.lexmark.emuls.profiler;

import java.sql.Connection;
import java.sql.Date;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.util.Arrays;
import java.util.LinkedList;

public class DBConnection extends LinkedList<String> {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	
	public static String newLine = System.getProperty("line.separator");
	protected static DBConnection instance = null;
	
	String dbUser = "tcprofiler";
	String dbPasswd = "********";
	String dbHost = System.getenv("TURTLE_DB");
	
	protected DBConnection() throws Exception { 
		try {
			Class.forName("oracle.jdbc.driver.OracleDriver");
		} catch (ClassNotFoundException e) {
			System.out.println("Where is your Oracle JDBC Driver?");
			throw e;
		}
	}
	
	public String getDbUser() {
		return dbUser;
	}

	public void setDbUser(String dbUser) {
		this.dbUser = dbUser;
	}

	public String getDbPasswd() {
		return dbPasswd;
	}

	public void setDbPasswd(String dbPasswd) {
		this.dbPasswd = dbPasswd;
	}

	public String getDbHost() {
		return dbHost;
	}

	public void setDbHost(String dbHost) {
		this.dbHost = dbHost;
	}
	
	protected Connection getDbConn() {
		Connection connection = null; 
		try {
//			connection = DriverManager.getConnection(
//					"jdbc:oracle:thin:@emulator-win7:1521:xe", "tc_profiler", "tc_profiler");
			connection = DriverManager.getConnection(
					"jdbc:oracle:thin:@"+dbHost+":1521:xe", dbUser, dbPasswd);
			System.out.format(String.format("Connected to: %s@%s DB...", dbHost, dbUser));
		} catch (SQLException e) {
			System.out.println("Connection Failed! Check output console");
			e.printStackTrace();
		}
		return connection;
	}
	
	protected static String setParameters(PreparedStatement stmt, Object ... args) throws SQLException {
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
		return Arrays.asList(args).toString();
	}
	
	protected static boolean insert(Connection conn, String sql, Object ... args) throws SQLException {
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
	
	protected static boolean update(Connection conn, String sql, Object ... args) throws SQLException {
		return insert(conn, sql, args);
	}
	
	protected static Object sqlQuery(Class<?> type, Connection conn, String sql, Object ... args) throws SQLException {
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
	
	public boolean importFile(String file) throws Exception {
		throw new UnsupportedOperationException("no implemetation");
	}
}
