DO $$
DECLARE
    -- 🔹 Replace with actual tenant code
    v_tenant_code TEXT := '<KP-TENANT-CODE>';

    v_data JSONB := '[
    {
            "taskExternalCode": "eat_more_seed_and_nuts_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "10-01", "expiryDate": "10-31" },
					{ "startDate": "11-01", "expiryDate": "11-30" },
                    { "startDate": "12-01", "expiryDate": "12-31" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "eat_the_rain_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "07-01", "expiryDate": "07-31" },
                    { "startDate": "08-01", "expiryDate": "08-31" },
                    { "startDate": "09-01", "expiryDate": "09-30" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "take_a_stro_afte_a_meal_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "04-01", "expiryDate": "04-30" },
                    { "startDate": "05-01", "expiryDate": "05-31" },
                    { "startDate": "06-01", "expiryDate": "06-30" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "conn_with_thos_who_make_you_smil_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "10-01", "expiryDate": "10-31" },
					{ "startDate": "11-01", "expiryDate": "11-30" },
                    { "startDate": "12-01", "expiryDate": "12-31" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "live_a_life_of_grat_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "01-01", "expiryDate": "01-31" },
					{ "startDate": "02-01", "expiryDate": "02-28" },
                    { "startDate": "03-01", "expiryDate": "03-31" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "powe_down_befo_bed_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "07-01", "expiryDate": "07-31" },
                    { "startDate": "08-01", "expiryDate": "08-31" },
                    { "startDate": "09-01", "expiryDate": "09-30" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "volu_your_time_2026",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "04-01", "expiryDate": "04-30" },
                    { "startDate": "05-01", "expiryDate": "05-31" },
                    { "startDate": "06-01", "expiryDate": "06-30" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "play_week_heal_triv_2026",
            "recurrenceDefinitionJson": {
				"schedules": [
					{
					  "startDate": "01-01",
					  "expiryDate": "01-07"
					},
					{
					  "startDate": "01-08",
					  "expiryDate": "01-14"
					},
					{
					  "startDate": "01-15",
					  "expiryDate": "01-21"
					},
					{
					  "startDate": "01-22",
					  "expiryDate": "01-28"
					},
					{
					  "startDate": "01-29",
					  "expiryDate": "02-04"
					},
					{
					  "startDate": "02-05",
					  "expiryDate": "02-11"
					},
					{
					  "startDate": "02-12",
					  "expiryDate": "02-18"
					},
					{
					  "startDate": "02-19",
					  "expiryDate": "02-25"
					},
					{
					  "startDate": "02-26",
					  "expiryDate": "03-04"
					},
					{
					  "startDate": "03-05",
					  "expiryDate": "03-11"
					},
					{
					  "startDate": "03-12",
					  "expiryDate": "03-18"
					},
					{
					  "startDate": "03-19",
					  "expiryDate": "03-25"
					},
					{
					  "startDate": "03-26",
					  "expiryDate": "04-01"
					},
					{
					  "startDate": "04-02",
					  "expiryDate": "04-08"
					},
					{
					  "startDate": "04-09",
					  "expiryDate": "04-15"
					},
					{
					  "startDate": "04-16",
					  "expiryDate": "04-22"
					},
					{
					  "startDate": "04-23",
					  "expiryDate": "04-29"
					},
					{
					  "startDate": "04-30",
					  "expiryDate": "05-06"
					},
					{
					  "startDate": "05-07",
					  "expiryDate": "05-13"
					},
					{
					  "startDate": "05-14",
					  "expiryDate": "05-20"
					},
					{
					  "startDate": "05-21",
					  "expiryDate": "05-27"
					},
					{
					  "startDate": "05-28",
					  "expiryDate": "06-03"
					},
					{
					  "startDate": "06-04",
					  "expiryDate": "06-10"
					},
					{
					  "startDate": "06-11",
					  "expiryDate": "06-17"
					},
					{
					  "startDate": "06-18",
					  "expiryDate": "06-24"
					},
					{
					  "startDate": "06-25",
					  "expiryDate": "07-01"
					},
					{
					  "startDate": "07-02",
					  "expiryDate": "07-08"
					},
					{
					  "startDate": "07-09",
					  "expiryDate": "07-15"
					},
					{
					  "startDate": "07-16",
					  "expiryDate": "07-22"
					},
					{
					  "startDate": "07-23",
					  "expiryDate": "07-29"
					},
					{
					  "startDate": "07-30",
					  "expiryDate": "08-05"
					},
					{
					  "startDate": "08-06",
					  "expiryDate": "08-12"
					},
					{
					  "startDate": "08-13",
					  "expiryDate": "08-19"
					},
					{
					  "startDate": "08-20",
					  "expiryDate": "08-26"
					},
					{
					  "startDate": "08-27",
					  "expiryDate": "09-02"
					},
					{
					  "startDate": "09-03",
					  "expiryDate": "09-09"
					},
					{
					  "startDate": "09-10",
					  "expiryDate": "09-16"
					},
					{
					  "startDate": "09-17",
					  "expiryDate": "09-23"
					},
					{
					  "startDate": "09-24",
					  "expiryDate": "09-30"
					},
					{
					  "startDate": "10-01",
					  "expiryDate": "10-07"
					},
					{
					  "startDate": "10-08",
					  "expiryDate": "10-14"
					},
					{
					  "startDate": "10-15",
					  "expiryDate": "10-21"
					},
					{
					  "startDate": "10-22",
					  "expiryDate": "10-28"
					},
					{
					  "startDate": "10-29",
					  "expiryDate": "11-04"
					},
					{
					  "startDate": "11-05",
					  "expiryDate": "11-11"
					},
					{
					  "startDate": "11-12",
					  "expiryDate": "11-18"
					},
					{
					  "startDate": "11-19",
					  "expiryDate": "11-25"
					},
					{
					  "startDate": "11-26",
					  "expiryDate": "12-02"
					},
					{
					  "startDate": "12-03",
					  "expiryDate": "12-09"
					},
					{
					  "startDate": "12-10",
					  "expiryDate": "12-16"
					},
					{
					  "startDate": "12-17",
					  "expiryDate": "12-23"
					},
					{
					  "startDate": "12-24",
					  "expiryDate": "12-30"
					},
					{
					  "startDate": "12-31",
					  "expiryDate": "12-31"
					}
		  ],
		  "recurrenceType": "SCHEDULE"
		}	
		},
        {
            "taskExternalCode": "play_dail_heal_triv_2026",
            "recurrenceDefinitionJson": {
			  "schedules": [
				{
				  "startDate": "01-01",
				  "expiryDate": "01-02"
				},
				{
				  "startDate": "01-02",
				  "expiryDate": "01-03"
				},
				{
				  "startDate": "01-03",
				  "expiryDate": "01-04"
				},
				{
				  "startDate": "01-04",
				  "expiryDate": "01-05"
				},
				{
				  "startDate": "01-05",
				  "expiryDate": "01-06"
				},
				{
				  "startDate": "01-06",
				  "expiryDate": "01-07"
				},
				{
				  "startDate": "01-07",
				  "expiryDate": "01-08"
				},
				{
				  "startDate": "01-08",
				  "expiryDate": "01-09"
				},
				{
				  "startDate": "01-09",
				  "expiryDate": "01-10"
				},
				{
				  "startDate": "01-10",
				  "expiryDate": "01-11"
				},
				{
				  "startDate": "01-11",
				  "expiryDate": "01-12"
				},
				{
				  "startDate": "01-12",
				  "expiryDate": "01-13"
				},
				{
				  "startDate": "01-13",
				  "expiryDate": "01-14"
				},
				{
				  "startDate": "01-14",
				  "expiryDate": "01-15"
				},
				{
				  "startDate": "01-15",
				  "expiryDate": "01-16"
				},
				{
				  "startDate": "01-16",
				  "expiryDate": "01-17"
				},
				{
				  "startDate": "01-17",
				  "expiryDate": "01-18"
				},
				{
				  "startDate": "01-18",
				  "expiryDate": "01-19"
				},
				{
				  "startDate": "01-19",
				  "expiryDate": "01-20"
				},
				{
				  "startDate": "01-20",
				  "expiryDate": "01-21"
				},
				{
				  "startDate": "01-21",
				  "expiryDate": "01-22"
				},
				{
				  "startDate": "01-22",
				  "expiryDate": "01-23"
				},
				{
				  "startDate": "01-23",
				  "expiryDate": "01-24"
				},
				{
				  "startDate": "01-24",
				  "expiryDate": "01-25"
				},
				{
				  "startDate": "01-25",
				  "expiryDate": "01-26"
				},
				{
				  "startDate": "01-26",
				  "expiryDate": "01-27"
				},
				{
				  "startDate": "01-27",
				  "expiryDate": "01-28"
				},
				{
				  "startDate": "01-28",
				  "expiryDate": "01-29"
				},
				{
				  "startDate": "01-29",
				  "expiryDate": "01-30"
				},
				{
				  "startDate": "01-30",
				  "expiryDate": "01-31"
				},
				{
				  "startDate": "01-31",
				  "expiryDate": "02-01"
				},
				{
				  "startDate": "02-01",
				  "expiryDate": "02-02"
				},
				{
				  "startDate": "02-02",
				  "expiryDate": "02-03"
				},
				{
				  "startDate": "02-03",
				  "expiryDate": "02-04"
				},
				{
				  "startDate": "02-04",
				  "expiryDate": "02-05"
				},
				{
				  "startDate": "02-05",
				  "expiryDate": "02-06"
				},
				{
				  "startDate": "02-06",
				  "expiryDate": "02-07"
				},
				{
				  "startDate": "02-07",
				  "expiryDate": "02-08"
				},
				{
				  "startDate": "02-08",
				  "expiryDate": "02-09"
				},
				{
				  "startDate": "02-09",
				  "expiryDate": "02-10"
				},
				{
				  "startDate": "02-10",
				  "expiryDate": "02-11"
				},
				{
				  "startDate": "02-11",
				  "expiryDate": "02-12"
				},
				{
				  "startDate": "02-12",
				  "expiryDate": "02-13"
				},
				{
				  "startDate": "02-13",
				  "expiryDate": "02-14"
				},
				{
				  "startDate": "02-14",
				  "expiryDate": "02-15"
				},
				{
				  "startDate": "02-15",
				  "expiryDate": "02-16"
				},
				{
				  "startDate": "02-16",
				  "expiryDate": "02-17"
				},
				{
				  "startDate": "02-17",
				  "expiryDate": "02-18"
				},
				{
				  "startDate": "02-18",
				  "expiryDate": "02-19"
				},
				{
				  "startDate": "02-19",
				  "expiryDate": "02-20"
				},
				{
				  "startDate": "02-20",
				  "expiryDate": "02-21"
				},
				{
				  "startDate": "02-21",
				  "expiryDate": "02-22"
				},
				{
				  "startDate": "02-22",
				  "expiryDate": "02-23"
				},
				{
				  "startDate": "02-23",
				  "expiryDate": "02-24"
				},
				{
				  "startDate": "02-24",
				  "expiryDate": "02-25"
				},
				{
				  "startDate": "02-25",
				  "expiryDate": "02-26"
				},
				{
				  "startDate": "02-26",
				  "expiryDate": "02-27"
				},
				{
				  "startDate": "02-27",
				  "expiryDate": "02-28"
				},
				{
				  "startDate": "02-28",
				  "expiryDate": "03-01"
				},
				{
				  "startDate": "03-01",
				  "expiryDate": "03-02"
				},
				{
				  "startDate": "03-02",
				  "expiryDate": "03-03"
				},
				{
				  "startDate": "03-03",
				  "expiryDate": "03-04"
				},
				{
				  "startDate": "03-04",
				  "expiryDate": "03-05"
				},
				{
				  "startDate": "03-05",
				  "expiryDate": "03-06"
				},
				{
				  "startDate": "03-06",
				  "expiryDate": "03-07"
				},
				{
				  "startDate": "03-07",
				  "expiryDate": "03-08"
				},
				{
				  "startDate": "03-08",
				  "expiryDate": "03-09"
				},
				{
				  "startDate": "03-09",
				  "expiryDate": "03-10"
				},
				{
				  "startDate": "03-10",
				  "expiryDate": "03-11"
				},
				{
				  "startDate": "03-11",
				  "expiryDate": "03-12"
				},
				{
				  "startDate": "03-12",
				  "expiryDate": "03-13"
				},
				{
				  "startDate": "03-13",
				  "expiryDate": "03-14"
				},
				{
				  "startDate": "03-14",
				  "expiryDate": "03-15"
				},
				{
				  "startDate": "03-15",
				  "expiryDate": "03-16"
				},
				{
				  "startDate": "03-16",
				  "expiryDate": "03-17"
				},
				{
				  "startDate": "03-17",
				  "expiryDate": "03-18"
				},
				{
				  "startDate": "03-18",
				  "expiryDate": "03-19"
				},
				{
				  "startDate": "03-19",
				  "expiryDate": "03-20"
				},
				{
				  "startDate": "03-20",
				  "expiryDate": "03-21"
				},
				{
				  "startDate": "03-21",
				  "expiryDate": "03-22"
				},
				{
				  "startDate": "03-22",
				  "expiryDate": "03-23"
				},
				{
				  "startDate": "03-23",
				  "expiryDate": "03-24"
				},
				{
				  "startDate": "03-24",
				  "expiryDate": "03-25"
				},
				{
				  "startDate": "03-25",
				  "expiryDate": "03-26"
				},
				{
				  "startDate": "03-26",
				  "expiryDate": "03-27"
				},
				{
				  "startDate": "03-27",
				  "expiryDate": "03-28"
				},
				{
				  "startDate": "03-28",
				  "expiryDate": "03-29"
				},
				{
				  "startDate": "03-29",
				  "expiryDate": "03-30"
				},
				{
				  "startDate": "03-30",
				  "expiryDate": "03-31"
				},
				{
				  "startDate": "03-31",
				  "expiryDate": "04-01"
				},
				{
				  "startDate": "04-01",
				  "expiryDate": "04-02"
				},
				{
				  "startDate": "04-02",
				  "expiryDate": "04-03"
				},
				{
				  "startDate": "04-03",
				  "expiryDate": "04-04"
				},
				{
				  "startDate": "04-04",
				  "expiryDate": "04-05"
				},
				{
				  "startDate": "04-05",
				  "expiryDate": "04-06"
				},
				{
				  "startDate": "04-06",
				  "expiryDate": "04-07"
				},
				{
				  "startDate": "04-07",
				  "expiryDate": "04-08"
				},
				{
				  "startDate": "04-08",
				  "expiryDate": "04-09"
				},
				{
				  "startDate": "04-09",
				  "expiryDate": "04-10"
				},
				{
				  "startDate": "04-10",
				  "expiryDate": "04-11"
				},
				{
				  "startDate": "04-11",
				  "expiryDate": "04-12"
				},
				{
				  "startDate": "04-12",
				  "expiryDate": "04-13"
				},
				{
				  "startDate": "04-13",
				  "expiryDate": "04-14"
				},
				{
				  "startDate": "04-14",
				  "expiryDate": "04-15"
				},
				{
				  "startDate": "04-15",
				  "expiryDate": "04-16"
				},
				{
				  "startDate": "04-16",
				  "expiryDate": "04-17"
				},
				{
				  "startDate": "04-17",
				  "expiryDate": "04-18"
				},
				{
				  "startDate": "04-18",
				  "expiryDate": "04-19"
				},
				{
				  "startDate": "04-19",
				  "expiryDate": "04-20"
				},
				{
				  "startDate": "04-20",
				  "expiryDate": "04-21"
				},
				{
				  "startDate": "04-21",
				  "expiryDate": "04-22"
				},
				{
				  "startDate": "04-22",
				  "expiryDate": "04-23"
				},
				{
				  "startDate": "04-23",
				  "expiryDate": "04-24"
				},
				{
				  "startDate": "04-24",
				  "expiryDate": "04-25"
				},
				{
				  "startDate": "04-25",
				  "expiryDate": "04-26"
				},
				{
				  "startDate": "04-26",
				  "expiryDate": "04-27"
				},
				{
				  "startDate": "04-27",
				  "expiryDate": "04-28"
				},
				{
				  "startDate": "04-28",
				  "expiryDate": "04-29"
				},
				{
				  "startDate": "04-29",
				  "expiryDate": "04-30"
				},
				{
				  "startDate": "04-30",
				  "expiryDate": "05-01"
				},
				{
				  "startDate": "05-01",
				  "expiryDate": "05-02"
				},
				{
				  "startDate": "05-02",
				  "expiryDate": "05-03"
				},
				{
				  "startDate": "05-03",
				  "expiryDate": "05-04"
				},
				{
				  "startDate": "05-04",
				  "expiryDate": "05-05"
				},
				{
				  "startDate": "05-05",
				  "expiryDate": "05-06"
				},
				{
				  "startDate": "05-06",
				  "expiryDate": "05-07"
				},
				{
				  "startDate": "05-07",
				  "expiryDate": "05-08"
				},
				{
				  "startDate": "05-08",
				  "expiryDate": "05-09"
				},
				{
				  "startDate": "05-09",
				  "expiryDate": "05-10"
				},
				{
				  "startDate": "05-10",
				  "expiryDate": "05-11"
				},
				{
				  "startDate": "05-11",
				  "expiryDate": "05-12"
				},
				{
				  "startDate": "05-12",
				  "expiryDate": "05-13"
				},
				{
				  "startDate": "05-13",
				  "expiryDate": "05-14"
				},
				{
				  "startDate": "05-14",
				  "expiryDate": "05-15"
				},
				{
				  "startDate": "05-15",
				  "expiryDate": "05-16"
				},
				{
				  "startDate": "05-16",
				  "expiryDate": "05-17"
				},
				{
				  "startDate": "05-17",
				  "expiryDate": "05-18"
				},
				{
				  "startDate": "05-18",
				  "expiryDate": "05-19"
				},
				{
				  "startDate": "05-19",
				  "expiryDate": "05-20"
				},
				{
				  "startDate": "05-20",
				  "expiryDate": "05-21"
				},
				{
				  "startDate": "05-21",
				  "expiryDate": "05-22"
				},
				{
				  "startDate": "05-22",
				  "expiryDate": "05-23"
				},
				{
				  "startDate": "05-23",
				  "expiryDate": "05-24"
				},
				{
				  "startDate": "05-24",
				  "expiryDate": "05-25"
				},
				{
				  "startDate": "05-25",
				  "expiryDate": "05-26"
				},
				{
				  "startDate": "05-26",
				  "expiryDate": "05-27"
				},
				{
				  "startDate": "05-27",
				  "expiryDate": "05-28"
				},
				{
				  "startDate": "05-28",
				  "expiryDate": "05-29"
				},
				{
				  "startDate": "05-29",
				  "expiryDate": "05-30"
				},
				{
				  "startDate": "05-30",
				  "expiryDate": "05-31"
				},
				{
				  "startDate": "05-31",
				  "expiryDate": "06-01"
				},
				{
				  "startDate": "06-01",
				  "expiryDate": "06-02"
				},
				{
				  "startDate": "06-02",
				  "expiryDate": "06-03"
				},
				{
				  "startDate": "06-03",
				  "expiryDate": "06-04"
				},
				{
				  "startDate": "06-04",
				  "expiryDate": "06-05"
				},
				{
				  "startDate": "06-05",
				  "expiryDate": "06-06"
				},
				{
				  "startDate": "06-06",
				  "expiryDate": "06-07"
				},
				{
				  "startDate": "06-07",
				  "expiryDate": "06-08"
				},
				{
				  "startDate": "06-08",
				  "expiryDate": "06-09"
				},
				{
				  "startDate": "06-09",
				  "expiryDate": "06-10"
				},
				{
				  "startDate": "06-10",
				  "expiryDate": "06-11"
				},
				{
				  "startDate": "06-11",
				  "expiryDate": "06-12"
				},
				{
				  "startDate": "06-12",
				  "expiryDate": "06-13"
				},
				{
				  "startDate": "06-13",
				  "expiryDate": "06-14"
				},
				{
				  "startDate": "06-14",
				  "expiryDate": "06-15"
				},
				{
				  "startDate": "06-15",
				  "expiryDate": "06-16"
				},
				{
				  "startDate": "06-16",
				  "expiryDate": "06-17"
				},
				{
				  "startDate": "06-17",
				  "expiryDate": "06-18"
				},
				{
				  "startDate": "06-18",
				  "expiryDate": "06-19"
				},
				{
				  "startDate": "06-19",
				  "expiryDate": "06-20"
				},
				{
				  "startDate": "06-20",
				  "expiryDate": "06-21"
				},
				{
				  "startDate": "06-21",
				  "expiryDate": "06-22"
				},
				{
				  "startDate": "06-22",
				  "expiryDate": "06-23"
				},
				{
				  "startDate": "06-23",
				  "expiryDate": "06-24"
				},
				{
				  "startDate": "06-24",
				  "expiryDate": "06-25"
				},
				{
				  "startDate": "06-25",
				  "expiryDate": "06-26"
				},
				{
				  "startDate": "06-26",
				  "expiryDate": "06-27"
				},
				{
				  "startDate": "06-27",
				  "expiryDate": "06-28"
				},
				{
				  "startDate": "06-28",
				  "expiryDate": "06-29"
				},
				{
				  "startDate": "06-29",
				  "expiryDate": "06-30"
				},
				{
				  "startDate": "06-30",
				  "expiryDate": "07-01"
				},
				{
				  "startDate": "07-01",
				  "expiryDate": "07-02"
				},
				{
				  "startDate": "07-02",
				  "expiryDate": "07-03"
				},
				{
				  "startDate": "07-03",
				  "expiryDate": "07-04"
				},
				{
				  "startDate": "07-04",
				  "expiryDate": "07-05"
				},
				{
				  "startDate": "07-05",
				  "expiryDate": "07-06"
				},
				{
				  "startDate": "07-06",
				  "expiryDate": "07-07"
				},
				{
				  "startDate": "07-07",
				  "expiryDate": "07-08"
				},
				{
				  "startDate": "07-08",
				  "expiryDate": "07-09"
				},
				{
				  "startDate": "07-09",
				  "expiryDate": "07-10"
				},
				{
				  "startDate": "07-10",
				  "expiryDate": "07-11"
				},
				{
				  "startDate": "07-11",
				  "expiryDate": "07-12"
				},
				{
				  "startDate": "07-12",
				  "expiryDate": "07-13"
				},
				{
				  "startDate": "07-13",
				  "expiryDate": "07-14"
				},
				{
				  "startDate": "07-14",
				  "expiryDate": "07-15"
				},
				{
				  "startDate": "07-15",
				  "expiryDate": "07-16"
				},
				{
				  "startDate": "07-16",
				  "expiryDate": "07-17"
				},
				{
				  "startDate": "07-17",
				  "expiryDate": "07-18"
				},
				{
				  "startDate": "07-18",
				  "expiryDate": "07-19"
				},
				{
				  "startDate": "07-19",
				  "expiryDate": "07-20"
				},
				{
				  "startDate": "07-20",
				  "expiryDate": "07-21"
				},
				{
				  "startDate": "07-21",
				  "expiryDate": "07-22"
				},
				{
				  "startDate": "07-22",
				  "expiryDate": "07-23"
				},
				{
				  "startDate": "07-23",
				  "expiryDate": "07-24"
				},
				{
				  "startDate": "07-24",
				  "expiryDate": "07-25"
				},
				{
				  "startDate": "07-25",
				  "expiryDate": "07-26"
				},
				{
				  "startDate": "07-26",
				  "expiryDate": "07-27"
				},
				{
				  "startDate": "07-27",
				  "expiryDate": "07-28"
				},
				{
				  "startDate": "07-28",
				  "expiryDate": "07-29"
				},
				{
				  "startDate": "07-29",
				  "expiryDate": "07-30"
				},
				{
				  "startDate": "07-30",
				  "expiryDate": "07-31"
				},
				{
				  "startDate": "07-31",
				  "expiryDate": "08-01"
				},
				{
				  "startDate": "08-01",
				  "expiryDate": "08-02"
				},
				{
				  "startDate": "08-02",
				  "expiryDate": "08-03"
				},
				{
				  "startDate": "08-03",
				  "expiryDate": "08-04"
				},
				{
				  "startDate": "08-04",
				  "expiryDate": "08-05"
				},
				{
				  "startDate": "08-05",
				  "expiryDate": "08-06"
				},
				{
				  "startDate": "08-06",
				  "expiryDate": "08-07"
				},
				{
				  "startDate": "08-07",
				  "expiryDate": "08-08"
				},
				{
				  "startDate": "08-08",
				  "expiryDate": "08-09"
				},
				{
				  "startDate": "08-09",
				  "expiryDate": "08-10"
				},
				{
				  "startDate": "08-10",
				  "expiryDate": "08-11"
				},
				{
				  "startDate": "08-11",
				  "expiryDate": "08-12"
				},
				{
				  "startDate": "08-12",
				  "expiryDate": "08-13"
				},
				{
				  "startDate": "08-13",
				  "expiryDate": "08-14"
				},
				{
				  "startDate": "08-14",
				  "expiryDate": "08-15"
				},
				{
				  "startDate": "08-15",
				  "expiryDate": "08-16"
				},
				{
				  "startDate": "08-16",
				  "expiryDate": "08-17"
				},
				{
				  "startDate": "08-17",
				  "expiryDate": "08-18"
				},
				{
				  "startDate": "08-18",
				  "expiryDate": "08-19"
				},
				{
				  "startDate": "08-19",
				  "expiryDate": "08-20"
				},
				{
				  "startDate": "08-20",
				  "expiryDate": "08-21"
				},
				{
				  "startDate": "08-21",
				  "expiryDate": "08-22"
				},
				{
				  "startDate": "08-22",
				  "expiryDate": "08-23"
				},
				{
				  "startDate": "08-23",
				  "expiryDate": "08-24"
				},
				{
				  "startDate": "08-24",
				  "expiryDate": "08-25"
				},
				{
				  "startDate": "08-25",
				  "expiryDate": "08-26"
				},
				{
				  "startDate": "08-26",
				  "expiryDate": "08-27"
				},
				{
				  "startDate": "08-27",
				  "expiryDate": "08-28"
				},
				{
				  "startDate": "08-28",
				  "expiryDate": "08-29"
				},
				{
				  "startDate": "08-29",
				  "expiryDate": "08-30"
				},
				{
				  "startDate": "08-30",
				  "expiryDate": "08-31"
				},
				{
				  "startDate": "08-31",
				  "expiryDate": "09-01"
				},
				{
				  "startDate": "09-01",
				  "expiryDate": "09-02"
				},
				{
				  "startDate": "09-02",
				  "expiryDate": "09-03"
				},
				{
				  "startDate": "09-03",
				  "expiryDate": "09-04"
				},
				{
				  "startDate": "09-04",
				  "expiryDate": "09-05"
				},
				{
				  "startDate": "09-05",
				  "expiryDate": "09-06"
				},
				{
				  "startDate": "09-06",
				  "expiryDate": "09-07"
				},
				{
				  "startDate": "09-07",
				  "expiryDate": "09-08"
				},
				{
				  "startDate": "09-08",
				  "expiryDate": "09-09"
				},
				{
				  "startDate": "09-09",
				  "expiryDate": "09-10"
				},
				{
				  "startDate": "09-10",
				  "expiryDate": "09-11"
				},
				{
				  "startDate": "09-11",
				  "expiryDate": "09-12"
				},
				{
				  "startDate": "09-12",
				  "expiryDate": "09-13"
				},
				{
				  "startDate": "09-13",
				  "expiryDate": "09-14"
				},
				{
				  "startDate": "09-14",
				  "expiryDate": "09-15"
				},
				{
				  "startDate": "09-15",
				  "expiryDate": "09-16"
				},
				{
				  "startDate": "09-16",
				  "expiryDate": "09-17"
				},
				{
				  "startDate": "09-17",
				  "expiryDate": "09-18"
				},
				{
				  "startDate": "09-18",
				  "expiryDate": "09-19"
				},
				{
				  "startDate": "09-19",
				  "expiryDate": "09-20"
				},
				{
				  "startDate": "09-20",
				  "expiryDate": "09-21"
				},
				{
				  "startDate": "09-21",
				  "expiryDate": "09-22"
				},
				{
				  "startDate": "09-22",
				  "expiryDate": "09-23"
				},
				{
				  "startDate": "09-23",
				  "expiryDate": "09-24"
				},
				{
				  "startDate": "09-24",
				  "expiryDate": "09-25"
				},
				{
				  "startDate": "09-25",
				  "expiryDate": "09-26"
				},
				{
				  "startDate": "09-26",
				  "expiryDate": "09-27"
				},
				{
				  "startDate": "09-27",
				  "expiryDate": "09-28"
				},
				{
				  "startDate": "09-28",
				  "expiryDate": "09-29"
				},
				{
				  "startDate": "09-29",
				  "expiryDate": "09-30"
				},
				{
				  "startDate": "09-30",
				  "expiryDate": "10-01"
				},
				{
				  "startDate": "10-01",
				  "expiryDate": "10-02"
				},
				{
				  "startDate": "10-02",
				  "expiryDate": "10-03"
				},
				{
				  "startDate": "10-03",
				  "expiryDate": "10-04"
				},
				{
				  "startDate": "10-04",
				  "expiryDate": "10-05"
				},
				{
				  "startDate": "10-05",
				  "expiryDate": "10-06"
				},
				{
				  "startDate": "10-06",
				  "expiryDate": "10-07"
				},
				{
				  "startDate": "10-07",
				  "expiryDate": "10-08"
				},
				{
				  "startDate": "10-08",
				  "expiryDate": "10-09"
				},
				{
				  "startDate": "10-09",
				  "expiryDate": "10-10"
				},
				{
				  "startDate": "10-10",
				  "expiryDate": "10-11"
				},
				{
				  "startDate": "10-11",
				  "expiryDate": "10-12"
				},
				{
				  "startDate": "10-12",
				  "expiryDate": "10-13"
				},
				{
				  "startDate": "10-13",
				  "expiryDate": "10-14"
				},
				{
				  "startDate": "10-14",
				  "expiryDate": "10-15"
				},
				{
				  "startDate": "10-15",
				  "expiryDate": "10-16"
				},
				{
				  "startDate": "10-16",
				  "expiryDate": "10-17"
				},
				{
				  "startDate": "10-17",
				  "expiryDate": "10-18"
				},
				{
				  "startDate": "10-18",
				  "expiryDate": "10-19"
				},
				{
				  "startDate": "10-19",
				  "expiryDate": "10-20"
				},
				{
				  "startDate": "10-20",
				  "expiryDate": "10-21"
				},
				{
				  "startDate": "10-21",
				  "expiryDate": "10-22"
				},
				{
				  "startDate": "10-22",
				  "expiryDate": "10-23"
				},
				{
				  "startDate": "10-23",
				  "expiryDate": "10-24"
				},
				{
				  "startDate": "10-24",
				  "expiryDate": "10-25"
				},
				{
				  "startDate": "10-25",
				  "expiryDate": "10-26"
				},
				{
				  "startDate": "10-26",
				  "expiryDate": "10-27"
				},
				{
				  "startDate": "10-27",
				  "expiryDate": "10-28"
				},
				{
				  "startDate": "10-28",
				  "expiryDate": "10-29"
				},
				{
				  "startDate": "10-29",
				  "expiryDate": "10-30"
				},
				{
				  "startDate": "10-30",
				  "expiryDate": "10-31"
				},
				{
				  "startDate": "10-31",
				  "expiryDate": "11-01"
				},
				{
				  "startDate": "11-01",
				  "expiryDate": "11-02"
				},
				{
				  "startDate": "11-02",
				  "expiryDate": "11-03"
				},
				{
				  "startDate": "11-03",
				  "expiryDate": "11-04"
				},
				{
				  "startDate": "11-04",
				  "expiryDate": "11-05"
				},
				{
				  "startDate": "11-05",
				  "expiryDate": "11-06"
				},
				{
				  "startDate": "11-06",
				  "expiryDate": "11-07"
				},
				{
				  "startDate": "11-07",
				  "expiryDate": "11-08"
				},
				{
				  "startDate": "11-08",
				  "expiryDate": "11-09"
				},
				{
				  "startDate": "11-09",
				  "expiryDate": "11-10"
				},
				{
				  "startDate": "11-10",
				  "expiryDate": "11-11"
				},
				{
				  "startDate": "11-11",
				  "expiryDate": "11-12"
				},
				{
				  "startDate": "11-12",
				  "expiryDate": "11-13"
				},
				{
				  "startDate": "11-13",
				  "expiryDate": "11-14"
				},
				{
				  "startDate": "11-14",
				  "expiryDate": "11-15"
				},
				{
				  "startDate": "11-15",
				  "expiryDate": "11-16"
				},
				{
				  "startDate": "11-16",
				  "expiryDate": "11-17"
				},
				{
				  "startDate": "11-17",
				  "expiryDate": "11-18"
				},
				{
				  "startDate": "11-18",
				  "expiryDate": "11-19"
				},
				{
				  "startDate": "11-19",
				  "expiryDate": "11-20"
				},
				{
				  "startDate": "11-20",
				  "expiryDate": "11-21"
				},
				{
				  "startDate": "11-21",
				  "expiryDate": "11-22"
				},
				{
				  "startDate": "11-22",
				  "expiryDate": "11-23"
				},
				{
				  "startDate": "11-23",
				  "expiryDate": "11-24"
				},
				{
				  "startDate": "11-24",
				  "expiryDate": "11-25"
				},
				{
				  "startDate": "11-25",
				  "expiryDate": "11-26"
				},
				{
				  "startDate": "11-26",
				  "expiryDate": "11-27"
				},
				{
				  "startDate": "11-27",
				  "expiryDate": "11-28"
				},
				{
				  "startDate": "11-28",
				  "expiryDate": "11-29"
				},
				{
				  "startDate": "11-29",
				  "expiryDate": "11-30"
				},
				{
				  "startDate": "11-30",
				  "expiryDate": "12-01"
				},
				{
				  "startDate": "12-01",
				  "expiryDate": "12-02"
				},
				{
				  "startDate": "12-02",
				  "expiryDate": "12-03"
				},
				{
				  "startDate": "12-03",
				  "expiryDate": "12-04"
				},
				{
				  "startDate": "12-04",
				  "expiryDate": "12-05"
				},
				{
				  "startDate": "12-05",
				  "expiryDate": "12-06"
				},
				{
				  "startDate": "12-06",
				  "expiryDate": "12-07"
				},
				{
				  "startDate": "12-07",
				  "expiryDate": "12-08"
				},
				{
				  "startDate": "12-08",
				  "expiryDate": "12-09"
				},
				{
				  "startDate": "12-09",
				  "expiryDate": "12-10"
				},
				{
				  "startDate": "12-10",
				  "expiryDate": "12-11"
				},
				{
				  "startDate": "12-11",
				  "expiryDate": "12-12"
				},
				{
				  "startDate": "12-12",
				  "expiryDate": "12-13"
				},
				{
				  "startDate": "12-13",
				  "expiryDate": "12-14"
				},
				{
				  "startDate": "12-14",
				  "expiryDate": "12-15"
				},
				{
				  "startDate": "12-15",
				  "expiryDate": "12-16"
				},
				{
				  "startDate": "12-16",
				  "expiryDate": "12-17"
				},
				{
				  "startDate": "12-17",
				  "expiryDate": "12-18"
				},
				{
				  "startDate": "12-18",
				  "expiryDate": "12-19"
				},
				{
				  "startDate": "12-19",
				  "expiryDate": "12-20"
				},
				{
				  "startDate": "12-20",
				  "expiryDate": "12-21"
				},
				{
				  "startDate": "12-21",
				  "expiryDate": "12-22"
				},
				{
				  "startDate": "12-22",
				  "expiryDate": "12-23"
				},
				{
				  "startDate": "12-23",
				  "expiryDate": "12-24"
				},
				{
				  "startDate": "12-24",
				  "expiryDate": "12-25"
				},
				{
				  "startDate": "12-25",
				  "expiryDate": "12-26"
				},
				{
				  "startDate": "12-26",
				  "expiryDate": "12-27"
				},
				{
				  "startDate": "12-27",
				  "expiryDate": "12-28"
				},
				{
				  "startDate": "12-28",
				  "expiryDate": "12-29"
				},
				{
				  "startDate": "12-29",
				  "expiryDate": "12-30"
				},
				{
				  "startDate": "12-30",
				  "expiryDate": "12-31"
				}
			  ],
			  "recurrenceType": "SCHEDULE"
}
        }
]';

    task_record JSONB;
    v_task_code TEXT;
    v_task_json JSONB;
    v_updated_count INT;

BEGIN
    -- 🔹 Loop through each JSON element
    FOR task_record IN
        SELECT * FROM jsonb_array_elements(v_data)
    LOOP
        v_task_code := task_record->>'taskExternalCode';
        v_task_json := task_record->'recurrenceDefinitionJson';

        -- 🔹 Update recurrence_definition_json if matching record exists
        UPDATE task.task_reward
        SET recurrence_definition_json = v_task_json,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE task_external_code = v_task_code
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND is_recurring = TRUE;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated recurrence_definition_json for task: % (tenant: %)', v_task_code, v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ task_reward not found for task_external_code: % with tenant_code: % and is_recurring = TRUE', v_task_code, v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '🎉 Recurrence update process completed!';
END $$;
