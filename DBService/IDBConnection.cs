using Client;
using System;
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
        int signUpUser(string name, string login, string password);

        [OperationContract]
        User SignInUser(string login, string password);

        [OperationContract]
        int createCrossword(Crossword cw);

        [OperationContract]
        List<string> getThemes();

        [OperationContract]
        int editCrossword(Crossword crossword);

        [OperationContract]
        int insertQuestions(List<QuestionAnswer> items, long owner_id);

        [OperationContract]
        List<QuestionAnswer> getCrosswordQuestions(long crosswirdID);

        [OperationContract]
        List<Crossword> filterCrosswordsByThemeName(FilterRequest filter);

        [OperationContract]
        long countFilteredCrosswords(FilterRequest filter);

        [OperationContract]
        void updateRathing(long crosswordId, int rathing);

        [OperationContract]
        void saveCoins(long uid, int coins);

        [OperationContract]
        void deleteCrosswordById(long id);

    }
}
