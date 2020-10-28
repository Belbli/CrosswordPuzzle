﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DBService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IDBConnection" в коде и файле конфигурации.
    [ServiceContract]
    public interface IDBConnection
    {
        [OperationContract]
        int GetSum(int a, int b);

        [OperationContract]
        int GetDiff(int a, int b);

        [OperationContract]
        string GetTableData();

        [OperationContract]
        int signUpUser(string name, string login, string password);

        [OperationContract]
        User SignInUser(string login, string password);

        [OperationContract]
        List<Crossword> getCrosswords(int offset, int count);

        [OperationContract]
        int createCrossword(Crossword cw);

        [OperationContract]
        List<string> getThemes();

        [OperationContract]
        List<Crossword> getUserCrosswords(int id);

        [OperationContract]
        int insertQuestions(List<QuestionAnswer> items, int owner_id);

        [OperationContract]
        List<QuestionAnswer> getCrosswordQuestions(int crosswirdID);
    }
}
