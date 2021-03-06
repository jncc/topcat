﻿using System;
using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IRecordService
    {
        RecordServiceResult Insert(Record record, UserInfo user);
        RecordServiceResult Update(Record record, UserInfo user);
    }
}