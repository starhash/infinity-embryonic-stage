using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Grammar;
using Infinity.Scripting.Utils;
using Infinity.Engine;
using Infinity.Engine.Data;
using Infinity.Scripting;
using System.Reflection;

namespace Infinity.Engine.Script
{
    public class ICASMInterpreter
    {
        private static Stack<ICASMDirective> _executionstack;
        public static Stack<ICASMDirective> ExecutionStack { get { if (_executionstack == null) _executionstack = new Stack<ICASMDirective>(); return _executionstack; } }

        private static Stack<Address> _memoryStack;
        public static Stack<Address> MemoryStack { get { if (_memoryStack == null) _memoryStack = new Stack<Address>(); return _memoryStack; } }

        private static int _temps = 0;
        public static int TemporaryVariableCount { get { return _temps; }  set { _temps = value; } }

        private static ICASMInterpreterMode _mode;
        private static bool _functionReturned;
        private static bool _executingFunction;
        private static ICASMFunction currentFunction;
        private static Data.Type currentType;
        public static ICASMInterpreterMode Mode { get { return _mode; } }

        public static ICASMExecutionResult LoadAndExecute(string path)
        {
            if (!new FileInfo(path).Exists) throw new FileNotFoundException("File not found - "+path);
            TextReader treader = new StreamReader(path);
            string[] statements = treader.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return ICASMInterpreter.Execute("/", statements);
        }

        public static ICASMExecutionResult Execute(Scope _currentScope, string[] statements, ICASMInterpreterMode mode = ICASMInterpreterMode.Normal)
        {
            ICASMExecutionResult result = new ICASMExecutionResult() { Success = true };
            for (int i = 0; i < statements.Length; i++)
            {
                string statement = statements[i];
                if (statement.StartsWith("#")) continue;
                if (_mode == ICASMInterpreterMode.Function)
                {
                    if (statement.Trim().Equals("end+"))
                    {
                        _mode = mode;
                        Data.Type type = TypeEngine.GetType(currentFunction.Address.Parent);
                        type.AddFunction(currentFunction);
                        currentFunction = null;
                        continue;
                    }
                    currentFunction.ExecutionQueue.Enqueue(statement);
                    continue;
                }
                else if (_mode == ICASMInterpreterMode.Fields)
                {
                    if (statement.Trim().Equals("end+"))
                    {
                        _mode = mode;
                        currentType = null;
                        continue;
                    }
                    currentType.MemberCreationQueue.Enqueue(statement);
                    continue;
                }
                ICASMDirective directive = ParseDirective(_currentScope, statement);
                ICASMExecutionResult temp_result = Execute(_currentScope, directive);
                if (temp_result.Data.ContainsKey("JumpDirective"))
                {
                    int step = (int)temp_result.Data["JumpDirective"];
                    i = i + step - 1;
                }
                if (!_functionReturned && !(_mode == ICASMInterpreterMode.Function))
                {
                    if (!(directive.Type != ICASMDirectiveType.ReflectDirective || directive.Tag != ICASMTagType.Help))
                    {
                        _functionReturned = false;
                    }
                    continue;
                }
                result = temp_result;
                _functionReturned = false;
            }
            return result;
        }
        public static ICASMDirective ParseDirective(Scope _currentScope, string statement)
        {
            statement = statement.Trim();
            LiteralGrammarElement lge = new LiteralGrammarElement("import", "reflect", "ewfc", "+var", "-var", "+pool", "-pool", 
                "+fields", "+field", "-field", "-fields", "+typespace", "-typespace", "+type", "-type", "-all",
                "+function", "-function", "call", "assign", "jump", "return", "clear", "if", "elseif", "else",
                "while", "repeat", "end+");
            TestResult<bool> directiveKeywordResult = lge.Validate(ref statement, true);
            string directiveKeyword = (string)directiveKeywordResult["Symbol"];
            statement = statement.Trim();
            ICASMDirectiveType type = ICASMDirective.GetTypeFromString(directiveKeyword);
            ICASMTagType tag = ICASMTagType.None;
            if (statement.StartsWith("-"))
            {
                LiteralGrammarElement tagChecker = new LiteralGrammarElement("-help", "-path", "-none", "-suppress", "-temporary", "-random", "-report", "-variable", "-pool", "-type", "-typespace");
                TestResult<bool> tagKeywordResult = tagChecker.Validate(ref statement, true);
                if (tagKeywordResult.Result)
                {
                    string tagKeyword = (string)tagKeywordResult["Symbol"];
                    tag = ICASMDirective.GetTagTypeFromString(tagKeyword);
                }
            }
            string innerstatement = null;
            statement = statement.Trim();
            if (type == ICASMDirectiveType.AssignDirective && statement.StartsWith("("))
            {
                innerstatement = statement.Trim().Substring(1, statement.LastIndexOf(')') - 1).Trim();
                statement = statement.Substring(statement.LastIndexOf(')') + 1).Trim();
                ICASMDirective innerdirective = ICASMInterpreter.ParseDirective(_currentScope, innerstatement);
                ICASMExecutionResult innerresult = ICASMInterpreter.Execute(_currentScope, innerdirective);
                if (innerresult.Data.ContainsKey("CallDirective"))
                {
                    statement = ((Address)innerresult.Data["CallDirective"]).FullPath + " " + statement;
                }
            }
            string[] paramstrs = statement.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ICASMDirectiveParameters parameters = new ICASMDirectiveParameters();
            string str = "";
            bool stringopen = false;
            foreach (string param in paramstrs)
            {
                ICASMValue value = null;
                if (!stringopen && param.StartsWith("\"") && !param.EndsWith("\""))
                {
                    stringopen = true;
                    str = param + " ";
                }
                else if (stringopen || (param.StartsWith("\"") && param.EndsWith("\"")))
                {
                    str = str + param + " ";
                    if (param.EndsWith("\""))
                    {
                        stringopen = false;
                        value = ICASMValue.ParseValue(str.Trim());
                        str = "";
                    }
                }
                else
                {
                    value = ICASMValue.ParseValue(param);
                }
                if (value != null) parameters.Add(value);
            }
            return new ICASMDirective(type, tag, parameters);
        }
        public static ICASMExecutionResult Execute(Scope _currentScope, ICASMDirective directive)
        {
            return Execute(_currentScope, directive.Type, directive.Tag, directive.Parameters);
        }
        public static ICASMExecutionResult Execute(Scope _currentScope, ICASMDirectiveType type, ICASMTagType tag, ICASMDirectiveParameters parameters)
        {
            ICASMExecutionResult result = new ICASMExecutionResult() { Success = true };
            ICASMValue varname, varvalue, varscope, vartype;
            Variable variable;
            Address address;
            string _ideoutput = "";
            switch (type)
            {
                #region VariableAddDirective
                case ICASMDirectiveType.VariableAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tVariableAddDirective, usage - \n\t+var <name> <?scope> <?type> <?value>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    vartype = null;
                    varvalue = new ICASMValue(ICASMValueType.Normal, null);
                    if(parameters.Count > 1)
                    {
                        varscope = parameters[1].Check(ICASMValueType.Address);
                        if(parameters.Count > 2)
                        {
                            vartype = parameters[2].Check(ICASMValueType.Address);
                            if (parameters.Count > 3)
                            {
                                varvalue = parameters[3].Check(ICASMValueType.Address, ICASMValueType.ExecutableResult, ICASMValueType.Identifier, ICASMValueType.Normal);
                            }
                        }
                    }
                    Data.Type varType = null;
                    if (vartype != null) varType = TypeEngine.GetType(Address.FromScope(new Scripting.Scope((string)vartype.Value)));
                    else varType = ICASMValue.GetTypeFromPrimitiveType(varvalue.PrimitiveType);
                    string[] memberCreation = varType.MemberCreationQueue.ToArray();
                    variable = new Variable(varvalue.Value, varType);
                    for (int i = 0; i<memberCreation.Length; i++)
                    {
                        ICASMDirective _temp_directive_function_member = ICASMInterpreter.ParseDirective(_currentScope, memberCreation[i]);
                        if (_temp_directive_function_member.Type == ICASMDirectiveType.VariableAddDirective)
                        {
                            _temp_directive_function_member.SetTag(ICASMTagType.AppendTuple);
                            string varscopestr = (string)varscope.Value;
                            if (!varscopestr.Equals("/")) varscopestr = varscopestr + "/";
                            _temp_directive_function_member.Parameters.Insert(1, new ICASMValue(ICASMValueType.Address, varscopestr + (string)varname.Value));
                            ICASMExecutionResult res = Execute(_currentScope, _temp_directive_function_member);
                            variable.TupleAddresses.Add((string)_temp_directive_function_member.Parameters[0].Value, (Address)res.Data["VariableAddDirective"]);
                        }
                    }
                    address = Address.FromScope(new Scripting.Scope((string)varscope.Value + "/" + (string)varname.Value));
                    if(tag == ICASMTagType.Suppress)
                    {
                        RuntimeEngine.CreatePool(address.Parent);
                        if(RuntimeEngine.GetPool(address.Parent).HasVariable(address.Name))
                        {
                            RuntimeEngine.GetPool(address.Parent).Pull(address.Name);
                        }
                    }
                    else if(tag == ICASMTagType.AppendTuple)
                    {
                        address = Address.FromScope(new Scripting.Scope((string)varscope.Value + "." + (string)varname.Value));
                    }
                    RuntimeEngine.PutVariable(address, variable);
                    result.Success = true;
                    result.Data.Add(type.ToString(), address);
                    break;
                #endregion
                #region VariableRemoveDirective
                case ICASMDirectiveType.VariableRemoveDirective:
                    if(tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tVariableRemoveDirective, usage - \n\t-var <name> <?scope>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1)
                    {
                        varscope = parameters[1].Check(ICASMValueType.Address);
                    }
                    VariablePool pool = RuntimeEngine.GetPool(Address.FromScope(new Scope((string)varscope.Value)));
                    variable = pool.Pull((string)varname.Value);
                    if(variable != null)
                    {
                        result.Success = true;
                        result.Data.Add(type.ToString(), new Address((string)varscope.Value, (string)varname.Value, AddressType.Variable));
                    }
                    break;
                #endregion
                #region VariablePoolAddDirective
                case ICASMDirectiveType.VariablePoolAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tVariablePoolAddDirective, usage - \n\t+pool <name> <?scope>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if(parameters.Count > 1)
                    {
                        varscope = parameters[1].Check(ICASMValueType.Address);
                    }
                    address = Address.FromScope(new Scope((string)varscope.Value + "/" + (string)varname.Value));
                    RuntimeEngine.CreatePool(address);
                    result.Success = true;
                    result.Data.Add(type.ToString(), address);
                    break;
                #endregion
                #region VariablePoolRemoveDirective
                case ICASMDirectiveType.VariablePoolRemoveDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tVariablePoolRemoveDirective, usage - \n\t-pool <name> <?scope>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1)
                    {
                        varscope = parameters[1].Check(ICASMValueType.Address);
                    }
                    address = Address.FromScope(new Scope((string)varscope.Value + "/" + (string)varname.Value));
                    RuntimeEngine.GetPool(address.Parent).PullPool(address.Name);
                    result.Success = true;
                    result.Data.Add(type.ToString(), address);
                    break;
                #endregion                
                #region CallDirective
                case ICASMDirectiveType.CallDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tCallDirective, usage - \n\tcall <type> <function name> <?parameters>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0];
                    if (varname.Type == ICASMValueType.Identifier && varname.Value.Equals("type(stack)"))
                        varname.Value = MemoryStack.Peek();
                    else varname = varname.Check(ICASMValueType.Address);
                    varscope = parameters[1];
                    if ((string)varscope.Value == "/") varscope.Type = ICASMValueType.FunctionIdentifier;
                    varscope = varscope.Check(ICASMValueType.FunctionIdentifier, ICASMValueType.Identifier);
                    if (parameters.Count > 2)
                    {
                        Address[] _addresses = new Address[parameters.Count - 2];
                        for(int i = 2; i<parameters.Count; i++)
                        {
                            if(parameters[i].Type == ICASMValueType.Normal)
                            {
                                Data.Type ttt = ICASMValue.GetTypeFromPrimitiveType(parameters[i].PrimitiveType);
                                variable = new Variable(parameters[i].Value, ttt);
                                RuntimeEngine.PutVariable("/$SYSTEM$_temp" + (_temps), variable);
                                _addresses[i - 2] = "/$SYSTEM$_temp" + _temps;
                                MemoryStack.Push(_addresses[i - 2]);
                                _temps++;
                            }
                            else if(parameters[i].Type == ICASMValueType.Address)
                            {
                                _addresses[i - 2] = (string)parameters[i].Value;
                            }
                            else if(parameters[i].Type == ICASMValueType.Identifier)
                            {
                                _addresses[i - 2] = "/" + (string)parameters[i].Value;
                            }
                        }
                        address = (string)varname.Value;
                        string temp_address = "/$SYSTEM$_temp" + (_temps);
                        _executingFunction = true;
                        TypeEngine.GetType(address).ExecuteFunction(temp_address, (string)varscope.Value, _addresses);
                        MemoryStack.Push(temp_address);
                        address = temp_address;
                        result.Success = true;
                        result.Data.Add(type.ToString(), (Address)temp_address);
                        _temps++;
                    }
                    break;
                #endregion
                #region AssignDirective
                case ICASMDirectiveType.AssignDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tAssignDirective, usage - \n\tassign <value> <address of variable>\n\t\t<value> - can be any primitive value, address, identifier or another nested <?call> or <?ewfc>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0];
                    varscope = parameters[1].Check(ICASMValueType.Identifier, ICASMValueType.Address);
                    address = (string)varscope.Value;
                    variable = new Variable();
                    Address addr = null;
                    if(varname.Type == ICASMValueType.Normal)
                    {
                        Data.Type ttt = ICASMValue.GetTypeFromPrimitiveType(varname.PrimitiveType);
                        variable = new Variable(varname.Value, ttt);
                    }
                    else if(varname.Type == ICASMValueType.Identifier)
                    {
                        addr = "/" + (string)varname.Value;
                        variable = RuntimeEngine.GetVariable(addr);
                    }
                    else if(varname.Type == ICASMValueType.Address)
                    {
                        addr = (string)varname.Value;
                        variable = RuntimeEngine.GetVariable(addr);
                    }
                    if (tag == ICASMTagType.Suppress)
                    {
                        RuntimeEngine.CreatePool(address.Parent);
                        VariablePool variablep = RuntimeEngine.GetPool(address.Parent);
                        if (variablep.HasVariable(address.Name))
                            variablep.Pull(address.Name);
                        RuntimeEngine.PutVariable(address, variable);
                    }
                    RuntimeEngine.SetVariable(address, variable);
                    result.Success = true;
                    result.Data.Add(type.ToString(), address);
                    break;
                #endregion
                #region FunctionAddDirective
                case ICASMDirectiveType.FunctionAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tFunctionAddDirective, usage - \n\t+function <name> <type address> <function type> <?parameter type addresses>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.FunctionIdentifier, ICASMValueType.Identifier);
                    if (varname == null && parameters[0].Value.Equals("/"))
                        varname = parameters[0];
                    varscope = parameters[1].Check(ICASMValueType.Address);
                    vartype = parameters[2].Check(ICASMValueType.Identifier);
                    List<Address> parametertypes = new List<Address>();
                    int j = 3;
                    while(j < parameters.Count && parameters[j].Type == ICASMValueType.Address)
                    {
                        parametertypes.Add((string)parameters[j].Value);
                        j++;
                    }
                    FunctionType _newFunctionType = (FunctionType) Enum.Parse(typeof(FunctionType), (string)vartype.Value);
                    ICASMFunction new_function = new ICASMFunction((string)varname.Value, (string)varname.Value,
                        _newFunctionType, (string)varscope.Value, parametertypes.ToArray());
                    _mode = ICASMInterpreterMode.Function;
                    currentFunction = new_function;
                    break;
                #endregion
                #region FunctionRemoveDirective
                case ICASMDirectiveType.FunctionRemoveDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tFunctionRemoveDirective, usage - \n\t+function <name> <type address>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    //To-do
                    break;
                #endregion
                #region FieldsAddDirective
                case ICASMDirectiveType.FieldsAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tFieldsAddDirective, usage - \n\t+fields <type address>\n\t\t<... var add statements>\n\tend+";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Address);
                    currentType = TypeEngine.GetType((string)varname.Value);
                    _mode = ICASMInterpreterMode.Fields;
                    break;
                #endregion
                #region TypeAddDirective
                case ICASMDirectiveType.TypeAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tTypeAddDirective, usage - \n\t+type <name> <typespace address>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1) varscope = parameters[1].Check(ICASMValueType.Address);
                    Data.Type t = new Data.Type((string)varname.Value, (string)varscope.Value);
                    TypeEngine.AddType(t.Address, t);
                    break;
                #endregion
                #region TypeRemoveDirective
                case ICASMDirectiveType.TypeRemoveDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tTypeRemoveDirective, usage - \n\t-type <name> <typespace address>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1) varscope = parameters[1].Check(ICASMValueType.Address);
                    TypeSpace from = TypeEngine.GetTypeSpace((string)varscope.Value);
                    if (from.HasType((string)varname.Value)) from.Pull((string)varname.Value);
                    break;
                #endregion
                #region TypespaceAddDirective
                case ICASMDirectiveType.TypespaceAddDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tTypespaceAddDirective, usage - \n\t+typespace <name> <?typespace parent>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1) varscope = parameters[1].Check(ICASMValueType.Address);
                    TypeSpace _toCreateTypespaceIn = TypeEngine.GetTypeSpace((string)varscope.Value);
                    _toCreateTypespaceIn.CreateTypeSpace((string)varname.Value);
                    break;
                #endregion
                #region TypespaceRemoveDirective
                case ICASMDirectiveType.TypespaceRemoveDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tTypespaceRemoveDirective, usage - \n\t-typespace <name> <?typespace parent>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Identifier);
                    varscope = new ICASMValue(ICASMValueType.Address, "/");
                    if (parameters.Count > 1) varscope = parameters[1].Check(ICASMValueType.Address);
                    TypeSpace _toRemoveTypespaceFrom = TypeEngine.GetTypeSpace((string)varscope.Value);
                    _toRemoveTypespaceFrom.PullTypeSpace((string)varname.Value);
                    break;
                #endregion
                #region ReturnDirective
                case ICASMDirectiveType.ReturnDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tReturnDirective, usage - \n\treturn <identifier/address/value>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    address = null;
                    varname = parameters[0].Check(ICASMValueType.Address, ICASMValueType.Identifier, ICASMValueType.Normal);
                    if (varname.Type == ICASMValueType.Address)
                    {
                        address = (string)varname.Value;
                    } 
                    else if(varname.Type == ICASMValueType.Identifier)
                    {
                        address = "/" + (string)varname.Value;
                    }
                    else if(varname.Type == ICASMValueType.Normal)
                    {
                        Data.Type ttt = ICASMValue.GetTypeFromPrimitiveType(varname.PrimitiveType);
                        variable = new Variable(varname.Value, ttt);
                        RuntimeEngine.PutVariable("/$SYSTEM$_temp" + (_temps), variable);
                        address = "/$SYSTEM$_temp" + _temps;
                        MemoryStack.Push(address);
                        _temps++;
                    }
                    if (address != null)
                    {
                        result.Success = true;
                        result.Data.Add(type.ToString(), address);
                    }
                    _functionReturned = true;
                    _executingFunction = false;
                    break;
                #endregion
                #region TypeAddDirective
                case ICASMDirectiveType.ImportDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tTypeAddDirective, usage - \n\timport <librarypath/filepath>\n\t\t-path - for importing a file";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Normal);
                    string _importPath = (string)varname.Value;
                    LoadAndExecute(_importPath.Trim());
                    break;
                #endregion
                #region EwfcDirective
                case ICASMDirectiveType.EwfcDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tEwfcDirective, usage - \n\tewfc <.net namespace> <function name> <?parameters>";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varscope = parameters[0].Check(ICASMValueType.Address);
                    varname = parameters[1].Check(ICASMValueType.Identifier);
                    _importPath = (string)varscope.Value;
                    string _importMethodName = (string)varname.Value;
                    _importPath = _importPath.Replace('/', '.').Trim('.');
                    object[] _parametersToCallingFunction = new object[parameters.Count - 2];
                    System.Type[] _parameterTypesToCallingFunction = new System.Type[parameters.Count - 2];
                    variable = new Variable();
                    for(int i = 2; i < parameters.Count; i++)
                    {
                        if (parameters[i].Type == ICASMValueType.Normal)
                        {
                            Data.Type ttt = ICASMValue.GetTypeFromPrimitiveType(parameters[i].PrimitiveType);
                            variable = new Variable(parameters[i].Value, ttt);
                        }
                        else if (parameters[i].Type == ICASMValueType.Address)
                        {
                            variable = RuntimeEngine.GetVariable((string)parameters[i].Value);
                        }
                        else if (parameters[i].Type == ICASMValueType.Identifier)
                        {
                            variable = RuntimeEngine.GetVariable("/" + (string)parameters[i].Value);
                        }
                        if (variable.Value == null) continue;
                        _parametersToCallingFunction[i - 2] = variable.Value;
                        _parameterTypesToCallingFunction[i - 2] = variable.Value.GetType();
                    }
                    System.Type _typeOfCallingMethod = System.Type.GetType(_importPath);
                    MethodInfo _callingMethodInfo = _typeOfCallingMethod.GetMethod(_importMethodName, _parameterTypesToCallingFunction);
                    object _methodCallResult = _callingMethodInfo.Invoke(null, _parametersToCallingFunction);
                    if (_methodCallResult != null)
                    {
                        string _methodResultValue = _methodCallResult.ToString();
                        ICASMValue icasm_methodReturnedValue = null;
                        Variable _methodCallResultVariable = null;
                        if ((icasm_methodReturnedValue = ICASMValue.ParseValue(_methodResultValue)) != null)
                        {
                            Data.Type ttt = ICASMValue.GetTypeFromPrimitiveType(icasm_methodReturnedValue.PrimitiveType);
                            _methodCallResultVariable = new Variable(icasm_methodReturnedValue.Value, ttt);
                        }
                        else
                        {
                            _methodCallResultVariable = new Variable(_methodCallResult, new Data.Type("Object", "/"));
                        }
                        address = "/$SYSTEM$_temp" + _temps;
                        RuntimeEngine.PutVariable(address, _methodCallResultVariable);
                        result.Success = true;
                        result.Data.Add(ICASMDirectiveType.CallDirective.ToString(), address);
                        MemoryStack.Push(address);
                        _temps++;
                    }
                    break;
                #endregion
                #region ReflectDirective
                case ICASMDirectiveType.ReflectDirective:
                    if (tag == ICASMTagType.Help)
                    {
                        _ideoutput = "\n\tReflectDirective, usage - \n\treflect <address>\n\t\t-variable - reflect variable\n\t\t-pool - reflect variable pool\n\t\t-typespace - reflect typespace\n\t\t-type - reflect type";
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                        break;
                    }
                    varname = parameters[0].Check(ICASMValueType.Address);
                    if (tag == ICASMTagType.Variable)
                    {
                        Variable _reflectVariable = RuntimeEngine.GetVariable((string)varname.Value);
                        _ideoutput = "\n\tReflect options - " + tag + ", " + _reflectVariable.Address;
                        _ideoutput += "\n\tVariable Address - " + _reflectVariable.Address;
                        _ideoutput += "\n\tVariable Value - " + _reflectVariable.Value;
                        if (_reflectVariable.Type != null)
                        {
                            _ideoutput += "\n\tVariable Type - " + _reflectVariable.Type.Address + "\n\tTuples - ";
                        }
                        foreach (Address _reflectVariableTuple in _reflectVariable.TupleAddresses.Values)
                        {
                            _ideoutput += "\n\t\t" + _reflectVariableTuple;
                        }
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                    }
                    else if (tag == ICASMTagType.Pool)
                    {
                        VariablePool _reflectPool = RuntimeEngine.GetPool((string)varname.Value);
                        _ideoutput = "\n\tReflect options - " + tag + ", " + _reflectPool.Address;
                        _ideoutput += "\n\tVariable Address - " + _reflectPool.Address;
                        _ideoutput += "\n\tVariablePools - " + _reflectPool.VariablePoolCount;
                        foreach (VariablePool _pool in _reflectPool.VariablePools.Values)
                        {
                            _ideoutput += "\n\t\t" + _pool.Address;
                        }
                        _ideoutput += "\n\tVariables - " + _reflectPool.VariableCount;
                        foreach (Variable _var in _reflectPool.Variables.Values)
                        {
                            _ideoutput += "\n\t\t" + _var.Address;
                        }
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                    }
                    else if (tag == ICASMTagType.Type)
                    {
                        Data.Type _reflectType = TypeEngine.GetType((string)varname.Value);
                        _ideoutput = "\n\tReflect options - " + tag + ", " + _reflectType.Address;
                        _ideoutput += "\n\tFunctions - " + _reflectType.FunctionCount;
                        foreach(FunctionOverloads f in _reflectType.Functions.Values)
                        {
                            _ideoutput += "\n\t\t" + f.AccessKey + " (overloads "+f.Overloads.Count+")";
                            for(int i = 0; i<f.Overloads.Count; i++)
                            {
                                _ideoutput += "\n\t\t\t" + i + " - ";
                                foreach(Address typse in f.Overloads[i].Parameters)
                                {
                                    _ideoutput += "["+typse.FullPath + "] ";
                                }
                            }
                        }
                        result.Data.Add("$IDE_OUTPUT$", _ideoutput);
                    }
                    else if(tag == ICASMTagType.Typespace)
                    {

                    }
                    break;
                #endregion
                #region JumpDirective
                case ICASMDirectiveType.JumpDirective:
                    varname = parameters[0];
                    int step = (int)varname.Value;
                    result.Success = true;
                    result.Data.Add(ICASMDirectiveType.JumpDirective.ToString(), step);
                    break;
                #endregion
            }
            if (!_executingFunction)
            while (MemoryStack.Count != 0)
            {
                Address addr = MemoryStack.Pop();
                if (result.Data.ContainsKey("CallDirective"))
                {
                    Address addr_c1 = (Address)result.Data["CallDirective"];
                    if (addr_c1.Equals(addr)) continue;
                }
                else if (result.Data.ContainsKey("ReturnDirective"))
                    if (result.Data["ReturnDirective"].Equals(addr.FullPath)) continue;
                VariablePool pool = RuntimeEngine.VariablePool;
                RuntimeEngine.GetPool(addr.Parent).Pull(addr.Name, true);
            }
            return result;
        }
    }
}
