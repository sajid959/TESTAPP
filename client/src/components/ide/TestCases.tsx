import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

interface TestCase {
  id: string;
  input: any;
  output: any;
  expected?: any;
  passed?: boolean;
  hidden?: boolean;
  runtime?: number;
  memory?: number;
  explanation?: string;
}

interface TestCasesProps {
  testCases: TestCase[];
  isRunning?: boolean;
  onRunTests?: () => void;
}

export function TestCases({ testCases, isRunning = false, onRunTests }: TestCasesProps) {
  const visibleTestCases = testCases.filter(tc => !tc.hidden);
  const passedTests = testCases.filter(tc => tc.passed === true).length;
  const totalTests = testCases.length;

  const formatTestCase = (input: any) => {
    if (typeof input === 'object') {
      return JSON.stringify(input, null, 2);
    }
    return String(input);
  };

  return (
    <div className="border-t border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
      <div className="px-6 py-3 border-b border-slate-200 dark:border-slate-700 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h4 className="font-semibold text-slate-900 dark:text-white">Test Cases</h4>
          {totalTests > 0 && (
            <Badge variant={passedTests === totalTests ? "default" : "secondary"}>
              {passedTests}/{totalTests} Passed
            </Badge>
          )}
        </div>
        
        {onRunTests && (
          <Button
            variant="outline"
            size="sm"
            onClick={onRunTests}
            disabled={isRunning}
            data-testid="button-run-tests"
          >
            {isRunning ? (
              <i className="fas fa-spinner fa-spin mr-2"></i>
            ) : (
              <i className="fas fa-play mr-2"></i>
            )}
            Run Tests
          </Button>
        )}
      </div>

      <div className="max-h-80 overflow-hidden">
        {visibleTestCases.length === 0 ? (
          <div className="p-6 text-center text-slate-500 dark:text-slate-400">
            <i className="fas fa-flask text-2xl mb-2"></i>
            <p>No test cases available</p>
          </div>
        ) : (
          <Tabs defaultValue="0" className="w-full">
            <TabsList className="grid grid-cols-3 w-full rounded-none">
              {visibleTestCases.slice(0, 3).map((testCase, index) => (
                <TabsTrigger 
                  key={testCase.id} 
                  value={index.toString()}
                  className="flex items-center space-x-2"
                  data-testid={`tab-test-case-${index}`}
                >
                  <span>Test Case {index + 1}</span>
                  {testCase.passed !== undefined && (
                    <i 
                      className={`fas ${
                        testCase.passed ? 'fa-check-circle text-success-600' : 'fa-times-circle text-error-600'
                      }`}
                    />
                  )}
                </TabsTrigger>
              ))}
            </TabsList>

            {visibleTestCases.slice(0, 3).map((testCase, index) => (
              <TabsContent key={testCase.id} value={index.toString()}>
                <ScrollArea className="h-64 p-6">
                  <div className="space-y-4">
                    {/* Input */}
                    <div>
                      <h5 className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                        Input:
                      </h5>
                      <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-3 font-mono text-sm">
                        <pre className="whitespace-pre-wrap">{formatTestCase(testCase.input)}</pre>
                      </div>
                    </div>

                    {/* Expected Output */}
                    <div>
                      <h5 className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                        Expected:
                      </h5>
                      <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-3 font-mono text-sm">
                        <pre className="whitespace-pre-wrap">{formatTestCase(testCase.expected || testCase.output)}</pre>
                      </div>
                    </div>

                    {/* Actual Output */}
                    {testCase.output !== undefined && testCase.expected && (
                      <div>
                        <h5 className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                          Output:
                        </h5>
                        <div className={`rounded-lg p-3 font-mono text-sm ${
                          testCase.passed 
                            ? 'bg-success-50 dark:bg-success-900/20 border border-success-200 dark:border-success-800' 
                            : 'bg-error-50 dark:bg-error-900/20 border border-error-200 dark:border-error-800'
                        }`}>
                          <pre className="whitespace-pre-wrap">{formatTestCase(testCase.output)}</pre>
                        </div>
                      </div>
                    )}

                    {/* Runtime & Memory */}
                    {(testCase.runtime !== undefined || testCase.memory !== undefined) && (
                      <div className="flex items-center space-x-4 text-sm text-slate-600 dark:text-slate-400">
                        {testCase.runtime !== undefined && (
                          <span>
                            <i className="fas fa-clock mr-1"></i>
                            Runtime: {testCase.runtime}ms
                          </span>
                        )}
                        {testCase.memory !== undefined && (
                          <span>
                            <i className="fas fa-memory mr-1"></i>
                            Memory: {testCase.memory}MB
                          </span>
                        )}
                      </div>
                    )}

                    {/* Explanation */}
                    {testCase.explanation && (
                      <div>
                        <h5 className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                          Explanation:
                        </h5>
                        <p className="text-sm text-slate-600 dark:text-slate-400">
                          {testCase.explanation}
                        </p>
                      </div>
                    )}
                  </div>
                </ScrollArea>
              </TabsContent>
            ))}
          </Tabs>
        )}
      </div>

      {/* Hidden test cases indicator */}
      {testCases.filter(tc => tc.hidden).length > 0 && (
        <div className="px-6 py-3 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800">
          <div className="flex items-center justify-between text-sm text-slate-600 dark:text-slate-400">
            <span>
              <i className="fas fa-lock mr-2"></i>
              {testCases.filter(tc => tc.hidden).length} hidden test case(s)
            </span>
            <span className="text-xs">Will be revealed after submission</span>
          </div>
        </div>
      )}
    </div>
  );
}
