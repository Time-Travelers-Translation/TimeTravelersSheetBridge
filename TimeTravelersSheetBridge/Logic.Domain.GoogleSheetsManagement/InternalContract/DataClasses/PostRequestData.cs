﻿namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class PostRequestData
    {
        public object[] Requests { get; set; }
        public bool IncludeSpreadsheetInResponse { get; set; }
        public string[] ResponseRanges { get; set; }
        public bool ResponseIncludeGridData { get; set; }
    }
}
