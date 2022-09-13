using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Pension_Contribution_Calculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text += " : v" + Assembly.GetExecutingAssembly().GetName().Version; // put in the version number

            //set up the UI
            lbl_weekly.Visible =
                txtbx_weekly.Visible =
                    lbl_monthly.Visible =
                        txtbx_monthly.Visible =
                            lbl_weekly.Visible =
                                txtbx_weekly.Visible =
                                    lbl_monthly.Visible =
                                        txtbx_monthly.Visible =
                                            lbl_tax_code.Visible =
                                                txt_tax_code.Visible = false;

            lbl_annually.Visible =
                txtbx_annually.Visible =
                    lbl_annually.Visible =
                        txtbx_annually.Visible =
                            lbl_personal_allowance.Visible =
                                txtbx_personal_allowance.Visible =
                                    lbl_flat_rate_expenses.Visible =
                                        txtbx_flat_rate_expenses.Visible = true;

        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_calc_Click(object sender, EventArgs e)
        {

            rchtxtbx_output.Clear();

           ////////////////////////////////////////////////////////////////
            /// Work out Gross Salary from data provided
            ////////////////////////////////////////////////////////////////

            double annual_gross_pay = 0;
            double monthly_gross_pay = 0;
            double weekly_gross_pay = 0;

            if (rdo_annually.Checked)
            {
                annual_gross_pay = float.Parse(txtbx_annually.Text);
                monthly_gross_pay = float.Parse(txtbx_annually.Text) / 12;
                weekly_gross_pay = float.Parse(txtbx_annually.Text) / 52;
            }
            else if (rdo_monthly.Checked)
            {
                monthly_gross_pay = float.Parse(txtbx_monthly.Text);
                weekly_gross_pay = (float.Parse(txtbx_monthly.Text) * 12) / 52;
                annual_gross_pay = float.Parse(txtbx_monthly.Text) * 12;
            }
            else if (rdo_weekly.Checked)
            {
                weekly_gross_pay = float.Parse(txtbx_weekly.Text);
                annual_gross_pay = float.Parse(txtbx_weekly.Text) * 52;
                monthly_gross_pay = float.Parse(txtbx_weekly.Text) * 12;
            }


            ////////////////////////////////////////////////////////
            /// Pension Contributions
            /// We take this off as Salary Sacrifice
            /// which lowers the tax rate you pay
            /// ////////////////////////////////////////////////////

            double company_pension = 0;
            double me_pension = 0;
            double avc_pension = 0;
            double total_pension = 0;
            double my_total_pension = 0;

            company_pension = monthly_gross_pay * float.Parse(txtbx_company_pension.Text) / 100.00;
            me_pension = monthly_gross_pay * float.Parse(txtbx_me_pension.Text) / 100.00;
            avc_pension = monthly_gross_pay * float.Parse(txtbx_avc_pension.Text) / 100.00;

            double total_pension_percentage = float.Parse(txtbx_me_pension.Text) +
                                              float.Parse(txtbx_avc_pension.Text);

            // Total is what we put into the pension pot
            total_pension = company_pension + me_pension + avc_pension;

            // My contribution comes off the Gross pay as salary sacrifice
            my_total_pension = me_pension + avc_pension;

            // Net pay is Gross minus the salary sacrifice
            double monthly_net_pay = monthly_gross_pay - my_total_pension;

           
            ////////////////////////////////////////////////////////
            /// NI Contributions
            ///
            /// < £1048 = 0%
            /// £1048 - £4189 = 13,25%
            /// > £4189 = 3.25%
            /// ////////////////////////////////////////////////////

           
            double ni_13_25 = 0.00;
            double ni_3_25 = 0.00;
            double ni_total = 0.00;

            if (monthly_net_pay > 4189)
            {
                ni_13_25 = 3141 * 0.1325;
                ni_3_25 = (monthly_net_pay - 4189) * 0.0325;
            }
            else
            {
                ni_13_25 = (monthly_net_pay - 1047.99) * 0.1325;
            }

            ni_total = ni_13_25 + ni_3_25;

           
            ////////////////////////////////////////////////////////
            /// Income Tax Contributions
            ///
            /// Take off the Salary sacrifice and then also the
            /// tax free amount and you end with the taxable pay.
            ///
            /// (£57270 - tax free amount) is taxed at 20%
            /// 
            /// next (£150000 - £57270) = £92730 is taxed at 40%
            /// 
            /// all the rest is taxed at 45%
            /// ////////////////////////////////////////////////////

            double tax_20 = 0.00;
            double tax_40 = 0.00;
            double tax_45 = 0.00;
            double tax_free_amount = 0.00;
            double tax_total = 0.00;
            double total_deduct = 0.00;
            double taxable_pay_20;



            if (rdobtn_no_taxcode.Checked)
            {
                tax_free_amount = float.Parse(txtbx_flat_rate_expenses.Text.Substring(1))
                                  + float.Parse(txtbx_personal_allowance.Text.Substring(1));
            }
            else
            {
                tax_free_amount = 10 * float.Parse(txt_tax_code.Text.Substring(0, txt_tax_code.Text.Length - 1));
            }

            double taxable_pay = (monthly_gross_pay - (my_total_pension + (tax_free_amount/12)))*12;

            taxable_pay_20 = 57270-tax_free_amount; 


            if (taxable_pay > 150000)
            {
                tax_20 = taxable_pay_20 * 0.2;
                tax_40 = 97230 * 0.4;
                tax_45 = (taxable_pay - 150000) * 0.45;
            }
            else if ((taxable_pay < 150000) && (taxable_pay > taxable_pay_20))
            {
                tax_20 = taxable_pay_20 * 0.2;
                tax_40 = (taxable_pay - taxable_pay_20) * 0.4;
                tax_40 = (annual_gross_pay - 50399) *0.4;
            }
            else if (taxable_pay <= taxable_pay_20)
            {
                tax_20 = taxable_pay * 0.2;
            }

            tax_total = tax_20 + tax_40 + tax_45;
            
            total_deduct = (tax_total / 12) + ni_total;


           ////////////////////////////////////////////////////////
            /// Bonus before 40% tax
            /// ////////////////////////////////////////////////////

            double bonus = (50270 - (monthly_net_pay * 12));
            
           ////////////////////////////////////////////////////////////////////////////////////////

            rchtxtbx_output.AppendText("Monthly Gross Pay = £" + String.Format("{0:.00}",Math.Round(monthly_gross_pay,2)) + "\r");
            rchtxtbx_output.AppendText("Monthly Take Home Pay = £" + String.Format("{0:.00}", Math.Round((monthly_net_pay - total_deduct),2)) + "\r\r");
            

            rchtxtbx_output.AppendText("Annual National Insurance = " + (12*ni_total).ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("Annual Tax Payable = " + tax_total.ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("Monthly Deductions = " + total_deduct.ToString("C", new CultureInfo("en-GB")) + "\r\r");


            rchtxtbx_output.AppendText("National Insurance @ 13.25% = " + ni_13_25.ToString("C", new CultureInfo("en-GB"))+ "\r");
            rchtxtbx_output.AppendText("National Insurance @ 3.25% = " + ni_3_25.ToString("C", new CultureInfo("en-GB"))+ "\r\r");
            

            rchtxtbx_output.AppendText("Tax @ 20 % = " + (tax_20 / 12).ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("Tax @ 40 % = " + (tax_40 / 12).ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("Tax @ 45 % = " + (tax_45 / 12).ToString("C", new CultureInfo("en-GB")) + "\r\r");


            rchtxtbx_output.AppendText("Company Pension = " + txtbx_company_pension.Text + "% = " + company_pension.ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("My Pension = " + txtbx_me_pension.Text + "% = " + me_pension.ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("AVC Pension = " + txtbx_avc_pension.Text + "% = " + avc_pension.ToString("C", new CultureInfo("en-GB")) + "\r");
            rchtxtbx_output.AppendText("Total Pension = " + total_pension_percentage.ToString() + "% = " + total_pension.ToString("C", new CultureInfo("en-GB")) + "\r\r");

            rchtxtbx_output.AppendText("Bonus can be = " + bonus.ToString("C", new CultureInfo("en-GB")) + 
            " or " + Math.Round(((bonus / (monthly_gross_pay * 12)) * 100), 2).ToString() + "% before 40% tax is taken from you.");
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            txtbx_annually.Text = "0";
            txtbx_monthly.Text = "0";
            txtbx_weekly.Text = "0";

            rchtxtbx_output.Clear();

            txtbx_company_pension.Text = "0";
            txtbx_me_pension.Text = "0";
            txtbx_avc_pension.Text = "0";

            txt_tax_code.Text = "1257L";
            txtbx_flat_rate_expenses.Text = "0";
            txtbx_personal_allowance.Text = "£12570";
        }


        private void rdo_weekly_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_weekly.Checked)
            {
               lbl_annually.Visible = 
                   txtbx_annually.Visible = 
                       lbl_monthly.Visible = 
                           txtbx_monthly.Visible = false;
                
               lbl_weekly.Visible = 
                   txtbx_weekly.Visible = true;
            }
        }

        private void rdo_monthly_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_monthly.Checked)
            {
                lbl_weekly.Visible = 
                    txtbx_weekly.Visible = 
                        lbl_annually.Visible = 
                            txtbx_annually.Visible = false;
                
                lbl_monthly.Visible = 
                    txtbx_monthly.Visible = true;
            }
        }

        private void rdo_annually_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_annually.Checked)
            {
                lbl_weekly.Visible =
                    txtbx_weekly.Visible =
                        lbl_monthly.Visible =
                            txtbx_monthly.Visible = false;

                lbl_annually.Visible =
                    txtbx_annually.Visible = true;
            }
        }
        
        private void rdobtn_no_taxcode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdobtn_no_taxcode.Checked)
            {
                lbl_personal_allowance.Visible =
                    txtbx_personal_allowance.Visible =
                        lbl_flat_rate_expenses.Visible =
                            txtbx_flat_rate_expenses.Visible = true;

                lbl_tax_code.Visible =
                    txt_tax_code.Visible = false;
            }
            else
            {
                lbl_personal_allowance.Visible =
                    txtbx_personal_allowance.Visible =
                        lbl_flat_rate_expenses.Visible =
                            txtbx_flat_rate_expenses.Visible = false;

                lbl_tax_code.Visible =
                    txt_tax_code.Visible = true;
            }
        }

        private void txtbx_annually_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
