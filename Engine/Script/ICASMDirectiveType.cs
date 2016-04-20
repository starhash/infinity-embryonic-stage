using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Script
{
    public enum ICASMDirectiveType
    {
        //Memory management directives
        VariableAddDirective,
        VariableRemoveDirective,
        VariablePoolAddDirective,
        VariablePoolRemoveDirective,
        TypeAddDirective,
        TypeRemoveDirective,
        FieldAddDirective,
        FieldRemoveDirective,
        FieldsAddDirective,
        FieldsRemoveDirective,
        TypespaceAddDirective,
        TypespaceRemoveDirective,
        AllRemoveDirective,
        FunctionAddDirective,
        FunctionRemoveDirective,

        //Memory or Functional action directives
        CallDirective,
        AssignDirective,
        JumpDirective,
        ReturnDirective,
        ClearDirective,

        //Decision making or Jump directives
        IfDirective,
        ElseIfDirective,
        ElseDirective,
        WhileDirective,
        RepeatDirective,

        //Other directives
        EndDirective,
        ImportDirective,
        EwfcDirective,
        ReflectDirective
    }
}
