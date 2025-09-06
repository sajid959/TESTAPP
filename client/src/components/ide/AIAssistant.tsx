import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { useToast } from '@/hooks/use-toast';
import { AIService, AIHintResponse, AIDebugResponse } from '@/services/aiService';
import { 
  Lightbulb, 
  Bug, 
  Zap, 
  ChevronDown, 
  ChevronRight, 
  Code, 
  Target,
  AlertTriangle,
  CheckCircle,
  Loader2
} from 'lucide-react';

interface AIAssistantProps {
  code: string;
  language: string;
  problemId: string;
  problemDescription: string;
  difficulty: string;
  testResults?: Array<{
    input: string;
    expectedOutput: string;
    actualOutput: string;
    passed: boolean;
  }>;
  lastError?: string;
}

export function AIAssistant({ 
  code, 
  language, 
  problemId, 
  problemDescription, 
  difficulty,
  testResults,
  lastError
}: AIAssistantProps) {
  const { toast } = useToast();
  const [activeTab, setActiveTab] = useState('hints');
  const [currentHint, setCurrentHint] = useState<AIHintResponse | null>(null);
  const [debugResults, setDebugResults] = useState<AIDebugResponse | null>(null);
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({});

  // Get AI hint mutation
  const getHintMutation = useMutation({
    mutationFn: async () => {
      const response = await AIService.getHint({
        problemId,
        problemDescription,
        currentCode: code,
        language,
        difficulty,
        hints: [] // Previous hints can be tracked here
      });
      return response;
    },
    onSuccess: (data) => {
      setCurrentHint(data);
      setActiveTab('hints');
      toast({
        title: 'AI Hint Generated',
        description: 'Check the hint to help you progress!',
      });
    },
    onError: () => {
      toast({
        title: 'Error',
        description: 'Failed to generate hint. Please try again.',
        variant: 'destructive'
      });
    }
  });

  // Debug code mutation
  const debugCodeMutation = useMutation({
    mutationFn: async () => {
      const response = await AIService.debugCode({
        code,
        language,
        error: lastError,
        testCases: testResults
      });
      return response;
    },
    onSuccess: (data) => {
      setDebugResults(data);
      setActiveTab('debug');
      toast({
        title: 'Code Analysis Complete',
        description: 'Check the debug results for suggestions!',
      });
    },
    onError: () => {
      toast({
        title: 'Error',
        description: 'Failed to analyze code. Please try again.',
        variant: 'destructive'
      });
    }
  });

  const toggleSection = (section: string) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  const getHintLevelColor = (level: string) => {
    switch (level) {
      case 'basic': return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'intermediate': return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      case 'advanced': return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      default: return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  };

  const getIssueIcon = (type: string) => {
    switch (type) {
      case 'syntax': return <AlertTriangle className="w-4 h-4 text-red-600" />;
      case 'logic': return <Bug className="w-4 h-4 text-orange-600" />;
      case 'performance': return <Zap className="w-4 h-4 text-blue-600" />;
      case 'edge_case': return <Target className="w-4 h-4 text-purple-600" />;
      default: return <AlertTriangle className="w-4 h-4 text-gray-600" />;
    }
  };

  return (
    <div className="space-y-4">
      {/* Action Buttons */}
      <div className="flex items-center gap-2">
        <Button 
          onClick={() => getHintMutation.mutate()}
          disabled={getHintMutation.isPending}
          variant="outline"
          size="sm"
        >
          {getHintMutation.isPending ? (
            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
          ) : (
            <Lightbulb className="w-4 h-4 mr-2" />
          )}
          Get Hint
        </Button>
        <Button 
          onClick={() => debugCodeMutation.mutate()}
          disabled={debugCodeMutation.isPending || !code.trim()}
          variant="outline"
          size="sm"
        >
          {debugCodeMutation.isPending ? (
            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
          ) : (
            <Bug className="w-4 h-4 mr-2" />
          )}
          Debug Code
        </Button>
      </div>

      {/* AI Assistant Tabs */}
      {(currentHint || debugResults) && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Zap className="w-5 h-5 text-blue-600" />
              AI Assistant
            </CardTitle>
            <CardDescription>
              AI-powered hints and debugging assistance
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Tabs value={activeTab} onValueChange={setActiveTab}>
              <TabsList className="grid w-full grid-cols-2">
                <TabsTrigger value="hints" className="flex items-center gap-2">
                  <Lightbulb className="w-4 h-4" />
                  Hints
                </TabsTrigger>
                <TabsTrigger value="debug" className="flex items-center gap-2">
                  <Bug className="w-4 h-4" />
                  Debug
                </TabsTrigger>
              </TabsList>

              {/* Hints Tab */}
              <TabsContent value="hints" className="mt-4">
                {currentHint ? (
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <Badge className={getHintLevelColor(currentHint.level)}>
                        {currentHint.level.toUpperCase()} HINT
                      </Badge>
                      <div className="text-sm text-gray-600">
                        Contextual hint based on your current code
                      </div>
                    </div>

                    <div className="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-lg border border-blue-200 dark:border-blue-800">
                      <p className="text-blue-900 dark:text-blue-100">
                        {currentHint.hint}
                      </p>
                    </div>

                    {currentHint.codeExample && (
                      <Collapsible 
                        open={expandedSections.codeExample} 
                        onOpenChange={() => toggleSection('codeExample')}
                      >
                        <CollapsibleTrigger asChild>
                          <Button variant="ghost" className="flex items-center gap-2 p-0">
                            {expandedSections.codeExample ? (
                              <ChevronDown className="w-4 h-4" />
                            ) : (
                              <ChevronRight className="w-4 h-4" />
                            )}
                            <Code className="w-4 h-4" />
                            View Code Example
                          </Button>
                        </CollapsibleTrigger>
                        <CollapsibleContent className="mt-2">
                          <div className="bg-gray-100 dark:bg-gray-800 p-3 rounded-md">
                            <pre className="text-sm overflow-x-auto">
                              <code>{currentHint.codeExample}</code>
                            </pre>
                          </div>
                        </CollapsibleContent>
                      </Collapsible>
                    )}

                    {currentHint.nextSteps && currentHint.nextSteps.length > 0 && (
                      <div>
                        <h4 className="font-medium text-gray-900 dark:text-white mb-2">
                          Next Steps:
                        </h4>
                        <ul className="space-y-1">
                          {currentHint.nextSteps.map((step, index) => (
                            <li key={index} className="flex items-start gap-2 text-sm">
                              <CheckCircle className="w-4 h-4 text-green-600 mt-0.5 flex-shrink-0" />
                              <span className="text-gray-700 dark:text-gray-300">{step}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <Lightbulb className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                    <p>Click "Get Hint" to receive AI-powered assistance</p>
                  </div>
                )}
              </TabsContent>

              {/* Debug Tab */}
              <TabsContent value="debug" className="mt-4">
                {debugResults ? (
                  <div className="space-y-4">
                    {/* Issues Section */}
                    {debugResults.issues && debugResults.issues.length > 0 && (
                      <div>
                        <h4 className="font-medium text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                          <AlertTriangle className="w-4 h-4 text-red-600" />
                          Issues Found ({debugResults.issues.length})
                        </h4>
                        <div className="space-y-3">
                          {debugResults.issues.map((issue, index) => (
                            <div key={index} className="border border-gray-200 dark:border-gray-700 rounded-lg p-3">
                              <div className="flex items-start gap-3">
                                {getIssueIcon(issue.type)}
                                <div className="flex-1">
                                  <div className="flex items-center gap-2 mb-1">
                                    <Badge variant="outline" className="text-xs">
                                      {issue.type.replace('_', ' ').toUpperCase()}
                                    </Badge>
                                    {issue.line && (
                                      <span className="text-xs text-gray-500">Line {issue.line}</span>
                                    )}
                                  </div>
                                  <p className="text-sm font-medium text-gray-900 dark:text-white mb-1">
                                    {issue.description}
                                  </p>
                                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                                    {issue.suggestion}
                                  </p>
                                  {issue.codeExample && (
                                    <div className="bg-gray-100 dark:bg-gray-800 p-2 rounded text-xs">
                                      <code>{issue.codeExample}</code>
                                    </div>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Suggestions Section */}
                    {debugResults.suggestions && debugResults.suggestions.length > 0 && (
                      <div>
                        <h4 className="font-medium text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                          <Target className="w-4 h-4 text-blue-600" />
                          General Suggestions
                        </h4>
                        <ul className="space-y-2">
                          {debugResults.suggestions.map((suggestion, index) => (
                            <li key={index} className="flex items-start gap-2 text-sm">
                              <CheckCircle className="w-4 h-4 text-blue-600 mt-0.5 flex-shrink-0" />
                              <span className="text-gray-700 dark:text-gray-300">{suggestion}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {/* Optimizations Section */}
                    {debugResults.optimizations && debugResults.optimizations.length > 0 && (
                      <div>
                        <h4 className="font-medium text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                          <Zap className="w-4 h-4 text-green-600" />
                          Performance Optimizations
                        </h4>
                        <ul className="space-y-2">
                          {debugResults.optimizations.map((optimization, index) => (
                            <li key={index} className="flex items-start gap-2 text-sm">
                              <Zap className="w-4 h-4 text-green-600 mt-0.5 flex-shrink-0" />
                              <span className="text-gray-700 dark:text-gray-300">{optimization}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {debugResults.issues.length === 0 && (
                      <div className="text-center py-6">
                        <CheckCircle className="w-12 h-12 mx-auto mb-4 text-green-600" />
                        <p className="text-green-600 font-medium">No major issues found!</p>
                        <p className="text-sm text-gray-600 mt-1">Your code looks good structurally.</p>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <Bug className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                    <p>Click "Debug Code" to analyze your solution</p>
                  </div>
                )}
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>
      )}
    </div>
  );
}