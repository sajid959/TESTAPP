import { useState, useEffect } from 'react';
import { useRoute } from 'wouter';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/hooks/useAuth';
import { useWebSocket } from '@/hooks/useWebSocket';
import { MonacoEditor } from '@/components/ide/MonacoEditor';
import { TestCases } from '@/components/ide/TestCases';
import { apiRequest } from '@/lib/queryClient';
import { Problem, Submission } from '@shared/schema';

export default function IDE() {
  const [match, params] = useRoute('/problem/:id');
  const problemId = params?.id;
  const { user } = useAuth();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const { joinProblem, updateCode, lastMessage } = useWebSocket();

  const [code, setCode] = useState('');
  const [language, setLanguage] = useState('python');
  const [isRunning, setIsRunning] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [testResults, setTestResults] = useState<any[]>([]);
  const [activeTab, setActiveTab] = useState('description');

  // Fetch problem data
  const { data: problem, isLoading: problemLoading } = useQuery<Problem>({
    queryKey: ['/api/problems', problemId],
    queryFn: () => fetch(`/api/problems/${problemId}`).then(res => res.json()),
    enabled: !!problemId,
  });

  // Fetch user submissions for this problem
  const { data: submissions = [] } = useQuery<Submission[]>({
    queryKey: ['/api/submissions', problemId],
    queryFn: () => {
      const params = new URLSearchParams({ problemId: problemId! });
      return fetch(`/api/submissions?${params.toString()}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
        },
      }).then(res => res.json());
    },
    enabled: !!problemId && !!user,
  });

  // Fetch discussions for this problem
  const { data: discussions = [] } = useQuery({
    queryKey: ['/api/discussions', problemId],
    queryFn: () => fetch(`/api/discussions/${problemId}`).then(res => res.json()),
    enabled: !!problemId,
  });

  // AI hint mutation
  const getHintMutation = useMutation({
    mutationFn: async () => {
      const response = await apiRequest('POST', '/api/ai/hint', {
        title: problem?.title,
        description: problem?.description,
        code,
        error: testResults.find(r => !r.passed)?.error,
      });
      return response.json();
    },
    onSuccess: (data) => {
      toast({
        title: 'AI Hint',
        description: data.hint,
      });
    },
  });

  // Run code mutation
  const runCodeMutation = useMutation({
    mutationFn: async () => {
      setIsRunning(true);
      // Simulate running code against visible test cases
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // Mock test results
      const results = problem?.testCases?.slice(0, 3).map((testCase: any, index: number) => ({
        id: `test-${index}`,
        input: testCase.input,
        output: testCase.output,
        expected: testCase.output,
        passed: Math.random() > 0.3, // Random pass/fail for demo
        runtime: Math.floor(Math.random() * 100) + 10,
        memory: Math.floor(Math.random() * 50) + 10,
        explanation: testCase.explanation,
      })) || [];
      
      setTestResults(results);
      return results;
    },
    onSuccess: (results) => {
      const passedCount = results.filter(r => r.passed).length;
      toast({
        title: `Test Results: ${passedCount}/${results.length} Passed`,
        description: passedCount === results.length ? 'All tests passed!' : 'Some tests failed.',
        variant: passedCount === results.length ? 'default' : 'destructive',
      });
    },
    onSettled: () => {
      setIsRunning(false);
    },
  });

  // Submit code mutation
  const submitCodeMutation = useMutation({
    mutationFn: async () => {
      const response = await apiRequest('POST', '/api/submissions', {
        problemId,
        code,
        language,
      });
      return response.json();
    },
    onSuccess: (submission) => {
      toast({
        title: 'Submission Received',
        description: 'Your code is being evaluated...',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/submissions', problemId] });
    },
    onError: (error: any) => {
      toast({
        title: 'Submission Failed',
        description: error.message || 'Failed to submit code',
        variant: 'destructive',
      });
    },
  });

  // Join problem room when component mounts
  useEffect(() => {
    if (problemId && user) {
      joinProblem(problemId);
    }
  }, [problemId, user, joinProblem]);

  // Handle WebSocket messages
  useEffect(() => {
    if (lastMessage) {
      if (lastMessage.type === 'submission_update') {
        const submission = lastMessage.data;
        if (submission.status === 'accepted') {
          toast({
            title: 'Submission Accepted!',
            description: `Runtime: ${submission.runtime}ms, Memory: ${submission.memory}MB`,
          });
        } else if (submission.status === 'rejected') {
          toast({
            title: 'Submission Rejected',
            description: submission.error || 'Your solution did not pass all test cases.',
            variant: 'destructive',
          });
        }
        setIsSubmitting(false);
      }
    }
  }, [lastMessage, toast]);

  // Update code in real-time via WebSocket
  const handleCodeChange = (newCode: string) => {
    setCode(newCode);
    if (problemId && user) {
      updateCode(problemId, newCode, language);
    }
  };

  const handleRun = () => {
    if (!code.trim()) {
      toast({
        title: 'Empty Code',
        description: 'Please write some code before running tests.',
        variant: 'destructive',
      });
      return;
    }
    runCodeMutation.mutate();
  };

  const handleSubmit = () => {
    if (!code.trim()) {
      toast({
        title: 'Empty Code',
        description: 'Please write some code before submitting.',
        variant: 'destructive',
      });
      return;
    }
    setIsSubmitting(true);
    submitCodeMutation.mutate();
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty?.toLowerCase()) {
      case 'easy': return 'difficulty-easy';
      case 'medium': return 'difficulty-medium';
      case 'hard': return 'difficulty-hard';
      default: return '';
    }
  };

  if (!match || !problemId) {
    return <div>Problem not found</div>;
  }

  if (problemLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
          <p>Loading problem...</p>
        </div>
      </div>
    );
  }

  if (!problem) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <i className="fas fa-exclamation-triangle text-4xl text-warning-500 mb-4"></i>
          <h2 className="text-xl font-semibold mb-2">Problem Not Found</h2>
          <p className="text-slate-600 dark:text-slate-400">The requested problem could not be found.</p>
        </div>
      </div>
    );
  }

  // Check if user can access this problem
  const canAccess = !problem.isPremium || user?.isPremium;
  if (!canAccess) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Card className="max-w-md mx-4">
          <CardContent className="pt-6 text-center">
            <i className="fas fa-crown text-4xl text-amber-500 mb-4"></i>
            <h2 className="text-xl font-semibold mb-2">Premium Problem</h2>
            <p className="text-slate-600 dark:text-slate-400 mb-4">
              This problem requires a premium subscription to access.
            </p>
            <Button className="bg-brand-600 hover:bg-brand-700">
              <i className="fas fa-star mr-2"></i>
              Upgrade to Premium
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col">
      {/* Header */}
      <div className="bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-700 px-6 py-4 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" onClick={() => window.history.back()} data-testid="button-back">
            <i className="fas fa-arrow-left text-lg"></i>
          </Button>
          <div className="flex items-center space-x-3">
            <h1 className="text-lg font-semibold text-slate-900 dark:text-white">
              {problem.title}
            </h1>
            <Badge className={getDifficultyColor(problem.difficulty)}>
              {problem.difficulty}
            </Badge>
            {problem.isPremium && (
              <Badge variant="outline">
                <i className="fas fa-crown mr-1 text-amber-500"></i>
                Premium
              </Badge>
            )}
          </div>
        </div>
        
        <div className="flex items-center space-x-3">
          <Button
            variant="outline"
            onClick={() => getHintMutation.mutate()}
            disabled={getHintMutation.isPending}
            data-testid="button-get-hint"
          >
            {getHintMutation.isPending ? (
              <i className="fas fa-spinner fa-spin mr-2"></i>
            ) : (
              <i className="fas fa-lightbulb mr-2"></i>
            )}
            AI Hint
          </Button>
          <Button
            variant="outline"
            onClick={handleRun}
            disabled={isRunning}
            className="bg-success-600 text-white hover:bg-success-700"
            data-testid="button-run-code"
          >
            {isRunning ? (
              <i className="fas fa-spinner fa-spin mr-2"></i>
            ) : (
              <i className="fas fa-play mr-2"></i>
            )}
            Run
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || submitCodeMutation.isPending}
            className="bg-brand-600 hover:bg-brand-700"
            data-testid="button-submit-code"
          >
            {isSubmitting || submitCodeMutation.isPending ? (
              <i className="fas fa-spinner fa-spin mr-2"></i>
            ) : (
              <i className="fas fa-check mr-2"></i>
            )}
            Submit
          </Button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex">
        {/* Left Panel - Problem Description */}
        <div className="w-1/2 border-r border-slate-200 dark:border-slate-700 flex flex-col">
          <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1">
            <div className="border-b border-slate-200 dark:border-slate-700 px-6 py-2">
              <TabsList>
                <TabsTrigger value="description" data-testid="tab-description">Description</TabsTrigger>
                <TabsTrigger value="submissions" data-testid="tab-submissions">
                  Submissions ({submissions.length})
                </TabsTrigger>
                <TabsTrigger value="discussions" data-testid="tab-discussions">
                  Discussions ({discussions.length})
                </TabsTrigger>
              </TabsList>
            </div>

            <TabsContent value="description" className="flex-1 m-0">
              <ScrollArea className="h-full">
                <div className="p-6 space-y-6">
                  <div className="prose prose-slate dark:prose-invert max-w-none">
                    <div className="mb-6">
                      <h2 className="text-xl font-semibold text-slate-900 dark:text-white mb-4">
                        Problem Description
                      </h2>
                      <div className="text-slate-700 dark:text-slate-300 whitespace-pre-wrap">
                        {problem.description}
                      </div>
                    </div>

                    {/* Examples */}
                    {Array.isArray(problem.examples) && problem.examples.length > 0 && (
                      <div className="space-y-4">
                        {problem.examples.map((example: any, index: number) => (
                          <div key={index}>
                            <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-3">
                              Example {index + 1}:
                            </h3>
                            <div className="bg-slate-50 dark:bg-slate-800 p-4 rounded-lg font-mono text-sm space-y-2">
                              <div><strong>Input:</strong> {example.input}</div>
                              <div><strong>Output:</strong> {example.output}</div>
                              {example.explanation && (
                                <div><strong>Explanation:</strong> {example.explanation}</div>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    )}

                    {/* Constraints */}
                    {problem.constraints && (
                      <div>
                        <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-3">
                          Constraints:
                        </h3>
                        <div className="bg-slate-50 dark:bg-slate-800 p-4 rounded-lg">
                          <pre className="text-sm text-slate-700 dark:text-slate-300 whitespace-pre-wrap font-mono">
                            {problem.constraints}
                          </pre>
                        </div>
                      </div>
                    )}

                    {/* Tags */}
                    {Array.isArray(problem.tags) && problem.tags.length > 0 && (
                      <div>
                        <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-3">
                          Topics:
                        </h3>
                        <div className="flex flex-wrap gap-2">
                          {problem.tags.map((tag: string) => (
                            <Badge key={tag} variant="outline">
                              {tag}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Hints */}
                    {Array.isArray(problem.hints) && problem.hints.length > 0 && (
                      <div>
                        <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-3">
                          Hints:
                        </h3>
                        <div className="space-y-2">
                          {problem.hints.map((hint: string, index: number) => (
                            <div key={index} className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-3">
                              <div className="flex items-start">
                                <i className="fas fa-lightbulb text-blue-600 mr-2 mt-1"></i>
                                <p className="text-sm text-blue-800 dark:text-blue-200">{hint}</p>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </ScrollArea>
            </TabsContent>

            <TabsContent value="submissions" className="flex-1 m-0">
              <ScrollArea className="h-full">
                <div className="p-6">
                  <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">
                    Your Submissions
                  </h3>
                  {submissions.length > 0 ? (
                    <div className="space-y-3">
                      {submissions.map((submission) => (
                        <Card key={submission.id}>
                          <CardContent className="p-4">
                            <div className="flex items-center justify-between mb-2">
                              <Badge
                                className={
                                  submission.status === 'accepted' ? 'difficulty-easy' :
                                  submission.status === 'rejected' ? 'difficulty-hard' : 'difficulty-medium'
                                }
                              >
                                {submission.status}
                              </Badge>
                              <span className="text-sm text-slate-500">
                                {new Date(submission.createdAt).toLocaleString()}
                              </span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                              <span className="text-slate-600 dark:text-slate-400">
                                {submission.language}
                              </span>
                              <div className="flex space-x-4 text-slate-600 dark:text-slate-400">
                                {submission.runtime && (
                                  <span>{submission.runtime}ms</span>
                                )}
                                {submission.memory && (
                                  <span>{submission.memory}MB</span>
                                )}
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      ))}
                    </div>
                  ) : (
                    <div className="text-center py-8">
                      <i className="fas fa-code text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
                      <p className="text-slate-600 dark:text-slate-400">
                        No submissions yet. Write some code and submit!
                      </p>
                    </div>
                  )}
                </div>
              </ScrollArea>
            </TabsContent>

            <TabsContent value="discussions" className="flex-1 m-0">
              <ScrollArea className="h-full">
                <div className="p-6">
                  <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-4">
                    Discussions
                  </h3>
                  {discussions.length > 0 ? (
                    <div className="space-y-4">
                      {discussions.map((discussion: any) => (
                        <Card key={discussion.id}>
                          <CardContent className="p-4">
                            <div className="flex items-start space-x-3">
                              <div className="w-8 h-8 bg-brand-100 dark:bg-brand-900 rounded-full flex items-center justify-center">
                                <span className="text-brand-600 text-sm font-semibold">
                                  {discussion.user?.username?.substring(0, 1).toUpperCase() || 'U'}
                                </span>
                              </div>
                              <div className="flex-1">
                                <div className="flex items-center space-x-2 mb-2">
                                  <span className="font-medium text-slate-900 dark:text-white">
                                    {discussion.user?.username || 'Anonymous'}
                                  </span>
                                  <span className="text-slate-500 text-sm">
                                    {new Date(discussion.createdAt).toLocaleString()}
                                  </span>
                                </div>
                                <p className="text-slate-700 dark:text-slate-300 text-sm">
                                  {discussion.content}
                                </p>
                                <div className="flex items-center space-x-4 mt-2">
                                  <button className="text-slate-500 hover:text-success-600 text-sm">
                                    <i className="far fa-thumbs-up mr-1"></i>
                                    {discussion.upvotes || 0}
                                  </button>
                                  <button className="text-slate-500 hover:text-slate-700 text-sm">
                                    Reply
                                  </button>
                                </div>
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      ))}
                    </div>
                  ) : (
                    <div className="text-center py-8">
                      <i className="fas fa-comments text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
                      <p className="text-slate-600 dark:text-slate-400">
                        No discussions yet. Be the first to start a conversation!
                      </p>
                    </div>
                  )}
                </div>
              </ScrollArea>
            </TabsContent>
          </Tabs>
        </div>

        {/* Right Panel - Code Editor */}
        <div className="w-1/2 flex flex-col">
          <div className="flex-1">
            <MonacoEditor
              value={code}
              onChange={handleCodeChange}
              language={language}
              onLanguageChange={setLanguage}
              onRun={handleRun}
              onSubmit={handleSubmit}
              isRunning={isRunning}
              isSubmitting={isSubmitting || submitCodeMutation.isPending}
            />
          </div>
          
          {/* Test Cases */}
          <TestCases
            testCases={testResults}
            isRunning={isRunning}
            onRunTests={handleRun}
          />
        </div>
      </div>
    </div>
  );
}
