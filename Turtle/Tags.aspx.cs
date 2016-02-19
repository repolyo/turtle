using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Tags : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void gridView_RowEditing(object sender, GridViewEditEventArgs e)
    {
        TagView.EditIndex = e.NewEditIndex;
        //loadStores();
    }
    protected void gridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        //DbConn.NewConnection(Config.getConnectionString());
        //string tag_id = TagView.DataKeys[e.RowIndex].Values["TID"].ToString();
        //TextBox tag_name = (TextBox)TagView.Rows[e.RowIndex].FindControl("ename");
        //TextBox tag_descr = (TextBox)TagView.Rows[e.RowIndex].FindControl("edesc");
        /*
        con.Open();
        SqlCommand cmd = new SqlCommand("update stores set tag_name='" + tag_name.Text + "', tag_descr='" + tag_descr.Text + "', city='" + city.Text + "', state='" + state.Text + "', zip='" + zip.Text + "' where stor_id=" + stor_id, con);
        cmd.ExecuteNonQuery();
        con.Close();
        lblmsg.BackColor = Color.Blue;
        lblmsg.ForeColor = Color.White;
        lblmsg.Text = stor_id + "        Updated successfully........    ";
        
        loadStores();
         * */

        //DbConn.Update("UPDATE TAGS SET TAG_NAME='{0}', TAG_DESCR='{1}' WHERE TID = {2}",
        //    tag_name.Text, tag_descr.Text, tag_id);
        //DbConn.Terminate();
        TagView.EditIndex = -1;
    }
    protected void gridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        TagView.EditIndex = -1;
        // loadStores();
    }
    protected void gridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        /*
        string stor_id = TagView.DataKeys[e.RowIndex].Values["stor_id"].ToString();
        con.Open();
        SqlCommand cmd = new SqlCommand("delete from stores where stor_id=" + stor_id, con);
        int result = cmd.ExecuteNonQuery();
        con.Close();
        if (result == 1)
        {
            loadStores();
            lblmsg.BackColor = Color.Red;
            lblmsg.ForeColor = Color.White;
            lblmsg.Text = stor_id + "      Deleted successfully.......    ";
        }
         * */
    }
    protected void gridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string stor_id = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "TID"));
            Button lnkbtnresult = (Button)e.Row.FindControl("ButtonDelete");
            if (lnkbtnresult != null)
            {
                lnkbtnresult.Attributes.Add("onclick", "javascript:return deleteConfirm('" + stor_id + "')");
            }
        }
    }
    protected void gridView_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        /*
        if (e.CommandName.Equals("New"))
        {
            TextBox instorid = (TextBox)TagView.FooterRow.FindControl("instorid");
            TextBox inname = (TextBox)TagView.FooterRow.FindControl("inname");
            TextBox inaddress = (TextBox)TagView.FooterRow.FindControl("inaddress");
            TextBox incity = (TextBox)TagView.FooterRow.FindControl("incity");
            TextBox instate = (TextBox)TagView.FooterRow.FindControl("instate");
            TextBox inzip = (TextBox)TagView.FooterRow.FindControl("inzip");
            con.Open();
            SqlCommand cmd =
                new SqlCommand(
                    "insert into stores(stor_id,tag_name,tag_descr,city,state,zip) values('" + instorid.Text + "','" +
                    inname.Text + "','" + inaddress.Text + "','" + incity.Text + "','" + instate.Text + "','" + inzip.Text + "')", con);
            int result = cmd.ExecuteNonQuery();
            con.Close();
            if (result == 1)
            {
                loadStores();
                lblmsg.BackColor = Color.Green;
                lblmsg.ForeColor = Color.White;
                lblmsg.Text = instorid.Text + "      Added successfully......    ";
            }
            else
            {
                lblmsg.BackColor = Color.Red;
                lblmsg.ForeColor = Color.White;
                lblmsg.Text = instorid.Text + " Error while adding row.....";
            }
        }*/
    }

    protected void AddTag_Click(object sender, EventArgs e)
    {
        /*
        Type cstype = this.GetType();

        // Get a ClientScriptManager reference from the Page class.
        ClientScriptManager cs = Page.ClientScript;

        // Check to see if the startup script is already registered.
        if (!cs.IsStartupScriptRegistered(cstype, "PopupScript"))
        {
            String cstext = "alert('" + sender.ToString() + "');";
            cs.RegisterStartupScript(cstype, "PopupScript", cstext, true);
        }*/
        DbConn.NewConnection(Config.getConnectionString());
        DbConn.Update("INSERT INTO TAGS (TAG_NAME, TAG_DESCR) VALUES ('-', '-')");
        DbConn.Terminate();
        TagView.DataBind();
    }  
}