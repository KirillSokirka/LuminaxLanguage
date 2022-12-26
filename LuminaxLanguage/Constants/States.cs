using LuminaxLanguage.Dto;

namespace LuminaxLanguage.Constants
{
    public static class States
    {
        public static Dictionary<StateTransition, int> StateTransitionsDictionary
            = new() {
                { new StateTransition(0, "Letter"), 1},
                { new StateTransition(0, "nl"), 5},
                { new StateTransition(0, "ws"), 0},
                { new StateTransition(0, "+"), 4},
                { new StateTransition(0, "-"), 4},
                { new StateTransition(0, "*"), 4},
                { new StateTransition(0, "/"), 4},
                { new StateTransition(0, "^"), 4},
                { new StateTransition(0, "("), 4},
                { new StateTransition(0, ")"), 4},
                { new StateTransition(0, "{"), 4},
                { new StateTransition(0, "}"), 4},
                { new StateTransition(0, ";"), 4},
                { new StateTransition(0, ","), 4},
                { new StateTransition(0, ":"), 4},
                { new StateTransition(0, "other"), 101},
                { new StateTransition(0, "="), 6},
                { new StateTransition(0, "<"), 6},
                { new StateTransition(0, ">"), 6},
                { new StateTransition(0, "!"), 9},
                { new StateTransition(0, "Digit"), 10},
                { new StateTransition(1, "Letter"), 1},
                { new StateTransition(1, "Digit"), 1},
                { new StateTransition(1, "other"), 3},
                { new StateTransition(3, "Digit"), 3},
                { new StateTransition(3, "other"), 4},
                { new StateTransition(6, "other"), 7},
                { new StateTransition(6, "="), 8},
                { new StateTransition(9, "="), 8},
                { new StateTransition(9, "other"), 102},
                { new StateTransition(10, "Digit"), 10},
                { new StateTransition(10, "other"), 12},
                { new StateTransition(10, "."), 11},
                { new StateTransition(11, "Digit"), 14},
                { new StateTransition(11, "other"), 103},
                { new StateTransition(14, "Digit"), 14},
                { new StateTransition(14, "other"), 16},
                { new StateTransition(14, "E"), 15},
                { new StateTransition(15, "-"), 17},
                { new StateTransition(17, "Digit"), 13},
                { new StateTransition(15, "other"), 104},
                { new StateTransition(17, "other"), 104},
                { new StateTransition(15, "Digit"), 13},
                { new StateTransition(13, "Digit"), 13},
                { new StateTransition(13, "other"), 18},
            };

        public static List<int> FinalStates = new() { 3, 4, 5, 7, 8, 12, 16, 18, 101, 102, 103, 104 };

        public static List<int> StatesToProcess = new() { 3, 4, 7, 12, 16 };

        public static List<int> ErrorStates = new() { 101, 102, 103, 104 };

        public const int InitState = 0;
    }
}
