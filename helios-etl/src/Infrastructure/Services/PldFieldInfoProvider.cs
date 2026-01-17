using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Helpers;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Etl.Infrastructure.Services
{
    public class PldFieldInfoProvider : IPldFieldInfoProvider
    {
        public PldFieldInfoProvider()
        {
            Init();
        }

        public List<PldFieldInfoDto> GetFieldInfo()
        {
            return _fieldInfo;
        }

        private void Init()
        {
            var idGen = new IdGenerator();

            // filter to avoid collisions
            HashSet<string> ids = new HashSet<string>();

            foreach (var field in _fieldInfo)
            {
                try
                {
                    if (field.FieldDescription != null)
                    {
                        var id = idGen.GenerateIdentifier(field.FieldDescription);
                        if (ids.Contains(id))
                        {
                            // generated ID already exists: handle collision
                            var id2 = id;
                            int next = 2;
                            while (ids.Contains(id2))
                            {
                                id2 = $"{id}_{next}";
                                next++;
                            }
                            id = id2;
                        }

                        field.FieldIdentifier = id.ToLower();
                        ids.Add(id);
                    }
                }
                catch (Exception)
                {
                    // TODO: log error
                    break;
                }
            }
        }

        private List<PldFieldInfoDto> _fieldInfo = new List<PldFieldInfoDto>()
        {
            new PldFieldInfoDto() { FieldLength = 11, FieldDescription = "MBI -A beneficiary's Medicare Beneficiary Identifier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 24, FieldDescription = "Last Name. A beneficiary's individual Last Name", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 15, FieldDescription = "First Name. A beneficiary's Individual First Name", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 22, FieldDescription = "City. A beneficiary's individual City of residence", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "State. A beneficiary's individual State of residence", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 5, FieldDescription = "Zip Code, A beneficiary's Individual Zip Code", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Sex. A beneficiary's gender assigned at birth", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 8, FieldDescription = "Birth Date. A beneficiary's individual Birth Date", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Plan ID Number. A beneficiary's assigned plan benefit number", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "SNP Enrollee Type. SNP benefit package at end of measurement year", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Member Months. The member month contribution (MMC) this member adds to the denominator, Each Medicare enrollee Ina given contract should be listed in the text file, The WIC is simply the number of months each Medicare member was enrolled In the contract in the measurement year. The MMC pertains only to Utilization measures; it does not apply to the Effectiveness of Care or Readmission measures, and does not vary by measure. The Enrollment by Product Line (ENP) measure should be used to determine member months.", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Colorectal Cancer Screening(COL): 46-49 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Colorectal Cancer Screening (COL): 46-49 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Colorectal Cancer Screening (COL); 50-75 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Colorectal Cancer Screening (COL): 50-75 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Breast Cancer Screening (BCS)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Breast Cancer Screening(BCS)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Osteoporosis Management in Women Who Had a Fracture (0 MW)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Osteoporosis Management in Women Who Had a Fracture (OMW)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Controlling High Blood Pressure (CBP)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Controlling High Blood Pressure (CBP)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Persistence of Beta-Blocker Treatment After a Heart Attack (PBH)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Persistence of Beta-Blocker Treatment After a Heart Attack (PBH)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Use of Spirometry Testing In the Assessment and Diagnosis of COPD (SPR)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1 , FieldDescription = "Numerator for Use of Spirometry Testing in the Assessment and Diagnosis of COPE (SPR)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Pharmacotherapy Management of COPD Exacerbation (PCE)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Pharmacotherapy Management of COPD Exacerbation (POE); Systemic Corticosteroid", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Pharmacotherapy Management of COPD Exacerbation (PCE): Bronchodilator", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Hospitalization for Mental Illness (FUN): 6-17 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUN): 6-17 years, 30-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUH): 6-17 years, 7-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Hospitalization for Mental illness (FUN): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUN): 18-64 years, 30-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUN): 18-64 years, 7-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Hospitalization for Mental Illness (FUN): 65-s years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUH): 65+ years, 30-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Hospitalization for Mental Illness (FUH); 65. years, 7-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Antidepressant Medication Management (AMM)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Antidepressant Medication Management (AMM): Effective Acute Phase Treatment", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Antidepressant Medication Management (AMM): Effective Continuation Phase Treatment", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Use of High-Risk Medications in Older Adults (DAE)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of High-Risk Medications in Older Adults (DAE); Rate 1.", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of High-Risk Medications In Older Adults (DAE): Rate 2", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of High-Risk Medications in Older Adults (DAE): Total Rate", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator 1 for Potentially Harmful Drug-Disease Interactions In Older Adults (DOE): Rate 1", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator 1 for Potentially Harmful Drug-Disease Interactions in Older Adults (DDE): Rate 1", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator 2 for Potentially Harmful Drug-Disease Interactions in Older Adults (DDE): Rate 2", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator 2 for Potentially Harmful Drug-Disease interactions in Older Adults (ODE): Rate 2", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator 3 for Potentially Harmful Drug-Disease Interactions in Older Adults (DDE): Rate 3", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator 3 for Potentially Harmful Drug-Disease Interactions in Older Adults (DOE); Rate 3", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 20-44", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 20-44", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 45-64", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 45-64", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 65+", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Adults' Access to Preventive/Ambulatory Health Services (AAP): Ages 65+", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Initiation and Engagement of Substance Use Disorder Treatment (IET): 13-17 years Alcohol use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for initiation and Engagement of Substance Use Disorder Treatment (IET): Initiation of SUD Treatment, 13-17 years Alcohol use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Engagement of SUD Treatment, 13-1.7 years Alcohol Use Disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Initiation and Engagement of Substance Use Disorder Treatment (IET): 13-17 years °plaid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Initiation of SUD Treatment, 13-17 years Opioid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Engagement of SUD Treatment, 13-17 years Opioid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for-Initiation and Engagement of Substance Use Disorder Treatment (IET): 13-17 years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Initiation of SUD Treatment, 13-17 years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (1ET); Engagement of SUD Treatment, 13-17 years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Initiation and Engagement of Substance Use Disorder Treatment (IET): 18+ years Alcohol use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Initiation of SUD Treatment, 18+ years Alcohol use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Engagement of SUD Treatment, 18+ years Alcohol use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Initiation and Engagement of Substance Use Disorder Treatment (IET): 18+ years Opioid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET); Initiation of SUD Treatment, 184-years Opioid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Engagement of SUD Treatment 18+ years Opioid use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Initiation and Engagement of Substance Use Disorder Treatment (IET): 18+ years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Initiation of SOD Treatment, 18+ years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Initiation and Engagement of Substance Use Disorder Treatment (IET): Engagement of SUD Treatment, 18+ years Other substance use disorder", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Bariatric weight loss surgery", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): CABG", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): PG", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP); Cardiac catheterization", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Carotid endarterectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Open Cholecystectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Laparoscopic cholecystectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Back surgery", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Abdominal hysterectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Vaginal hysterectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Prostatectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Total hip replacement", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Total knee replacement", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Mastectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Number of procedures for Frequency of Selected Procedures (FSP): Lumpectomy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Non-Recommended PSA-Based Screening in Older Men (PSA)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Non-Recommended PSA-Based Screening in Older Men (PSA)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Males 21-75, Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Males 21-75, Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Males 21-75, Stalin Adherence SO%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Males 21.75, Statin Adherence 80%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Females 40-75, Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Statin Therapy for Patients With Cardiovascular Disease (SPC); Females 40-75, Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Females 40-75, Statin Adherence 80%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Statin Therapy for Patients With Cardiovascular Disease (SPC): Females 40-75, Statin Adherence 80%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator-for Statin Therapy for Patients With Diabetes (SPD): Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Static Therapy for Patients With Diabetes (SPD): Received Statin Therapy", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Statin Therapy for Patients With Diabetes (SPD): Static Adherence 80%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Static Therapy for Patients With Diabetes (SPD): Static Adherence 80%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Acute Hospital Utilization (AHU): Non-Outliers", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Acute Hospital Utilization (AHU): Outliers", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Acute Hospital Utilization (AHU): Observed Discharges", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Acute Hospital Utilization (AHU): PUCD Comorbidity Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Acute Hospital Utilization (AHU): PPE, Age/Gender Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Acute Hospital Utilization (AHU): PUCD Comorbidity Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Acute Hospital Utilization (AHU): PUCD Age/Gender Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Emergency Department Utilization (EDU): Non-Outliers", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Emergency Department Utilization (EDU): Outliers", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 3, FieldDescription = "Emergency Department Utilization (EDU): Observed ED Visits", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Emergency Department Utilization (EDU): PPV Comorbidity Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Emergency Department Utilization (EDU): PPV Age/Gender Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Emergency Department Utilization (EDU): PUCV Comorbidity Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Emergency Department Utilization (EDU): PUCV Age/Gender Weight", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization  for Potentially Preventable Complications ii-IPC): Chronic ACSC Non-Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): Chronic ACSC Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): Acute ACSC Non-Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization for Potentially Preventable Complications (H PC): Acute ACSC Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization for Potentially Preventable Complications (H PC): Total ACSC Non-Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Hospitalization for Potentially Preventable Complications (H PC): Total ACSC Outlier", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): Observed Chronic ACSC Discharges", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): Observed Acute ACSC Discharges", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): Observed Total ACSC Discharges", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PPD Comorbidity Weight, Chronic ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PPD Comorbidity Weight, Acute ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PPO Comorbidity Weight, Total ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (FTC): PPD Age/Gender Weight, Chronic ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PPO Age/Gender Weight, Acute ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PPD Age/Gender Weight, Total ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PUCD Comorbidity Weight, Chronic ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PUCD Comorbidity Weight, Acute ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC); PUCD Comorbidity Weight, Total ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PUCD Age/Gender Weight, Chronic ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PUCD Age/Gender Weight, Acute ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 12, FieldDescription = "Hospitalization for Potentially Preventable Complications (HPC): PUCD Age/Gender Weight, Total ACSC", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 6.17 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Menial Illness (FUM): 6-17 years, 30- day follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 6-17 years, 7-day Follow-Up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 18-64 years, 30- day follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 18-64 years, 7- day Follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for Mental Illness (FUM); 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Mental Illness (FUM): 65+ years, 30-day Follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Mental illness (FUM): 65+ years, 7-day Follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for Substance Use (FUA); 13-17 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Substance Use (FUA): 13-17 years, 30- clay follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Substance Use (FUA): 13-17 years, 7- day follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for Substance Use (FUA): 18+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Substance Use (FUA): 18+ years, 30-day follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for Substance Use (FUA): 18+ years, 7-day follow-up", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Use of Opioids at High Dosage (HDO)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of Opioids at High Dosage (HOD)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Use of Opioids from Multiple Providers (UOP)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of Opioids from Multiple Providers (UOP): Multiple prescribers", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of Opioids from Multiple Providers (UOP): Multiple pharmacies", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Use of Opioids from Multiple Providers (UOP); Multiple prescribers and pharmacies", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC); 18-64 years, Notification of inpatient Admission", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 18-64 years, Notification of Inpatient Admission", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC): 18-64 years, Receipt of Discharge Information", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC):18-64 years, Receipt of Discharge Information", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC); 18-64 years, Patient Engagement after Inpatient Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC):18-64 years, Patient Engagement after Inpatient Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC): 18-64 years, Medication Reconciliation Post-Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 18-64 years, Medication Reconciliation Post-Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC); 65+ years, Notification of Inpatient Admission", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 65+ years, Notification of Inpatient Admission", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC): 65+ years, Receipt of Discharge information", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 65+ years, Receipt of Discharge Information", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC): 65+ years, Patient Engagement after Inpatient Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 65+ years, Patient Engagement after inpatient Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Transitions of Care (TRC): 65+ years, Medication Reconciliation Post-Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Transitions of Care (TRC): 65+ years, Medication Reconciliation Post-Discharge", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Follow-Up After Emergency Department Visit for People With Multiple High-Risk Chronic Conditions (FMC): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for People With Multiple High-Risk Chronic Conditions (FMC): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Denominator for Fallow-Up After Emergency Department Visit for People With Multiple High-Risk Chronic Conditions (FMC): 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 2, FieldDescription = "Numerator for Follow-Up After Emergency Department Visit for People With Multiple High-Risk Chronic Conditions (FMC); 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Adherence to Anti psychotic Medications for Individuals With Schizophrenia (SAA)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Adherence to Antipsychotic Medications for Individuals With Schizophrenia (SAA)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Kidney Health Evaluation for Patients with Diabetes (KED); 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Kidney Health Evaluation for Patients with Diabetes (KED): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Kidney Health Evaluation for Patients with Diabetes (KED): 65-74 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Kidney Health Evaluation for Patients with Diabetes (KED): 65-74 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Kidney Health Evaluation for Patients with Diabetes (KED); 75.85 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Kidney Health Evaluation for Patients with Diabetes (KED): 75-85 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Cardiac Rehabilitation (CRE): 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Initiation, 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Engagement 1, 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Engagement 2, 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Achievement, 18-64 years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Cardiac Rehabilitation (CRE): 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Initiation, 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Engagement 1, 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Engagement 2, 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator For Cardiac Rehabilitation (CRE): Achievement, 65+ years", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Osteoporosis Screening in Older Women (OSW)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Osteoporosis Screening ln Older Women (OSW)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Blood Pressure Control for Patients With Diabetes (BPD)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Blood Pressure Control for Patients With Diabetes (BPD)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Hemoglobin A1c Control for Patients With Diabetes (HBD)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Hemoglobin A1C Control for Patients With Diabetes (HBD): HbA1c Control <8%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Hemoglobin A1c Control for Patients With Diabetes (HBD): HbA1c Poor Control >9%", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Eye Exam for Patients With Diabetes (EED)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Eye Exam for Patients With Diabetes (EED)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Denominator for Pharmacotherapy for Opioid Use Disorder (POD)", FieldIdentifier = null },
            new PldFieldInfoDto() { FieldLength = 1, FieldDescription = "Numerator for Pharmacotherapy for Opioid Use Disorder (POD)", FieldIdentifier = null }
        };
    }
}
