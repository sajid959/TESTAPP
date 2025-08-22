import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { apiRequest } from '@/lib/queryClient';
import { Category } from '@shared/schema';

export function ProblemManager() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isAddingProblem, setIsAddingProblem] = useState(false);
  const [isGeneratingTests, setIsGeneratingTests] = useState(false);
  const [newProblem, setNewProblem] = useState({
    title: '',
    description: '',
    categoryId: '',
    difficulty: 'Easy' as 'Easy' | 'Medium' | 'Hard',
    tags: [] as string[],
    isPremium: false,
    hints: [] as string[],
    examples: [] as any[],
    constraints: '',
    testCases: [] as any[],
    solution: '',
  });

  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['/api/categories'],
  });

  const { data: problems = [], isLoading } = useQuery({
    queryKey: ['/api/problems'],
  });

  const createProblemMutation = useMutation({
    mutationFn: async (problemData: any) => {
      const response = await apiRequest('POST', '/api/problems', problemData);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Problem created successfully!',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/problems'] });
      setIsAddingProblem(false);
      setNewProblem({
        title: '',
        description: '',
        categoryId: '',
        difficulty: 'Easy',
        tags: [],
        isPremium: false,
        hints: [],
        examples: [],
        constraints: '',
        testCases: [],
        solution: '',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create problem',
        variant: 'destructive',
      });
    },
  });

  const generateTestCasesMutation = useMutation({
    mutationFn: async () => {
      const response = await apiRequest('POST', '/api/ai/generate-testcases', {
        title: newProblem.title,
        description: newProblem.description,
        constraints: newProblem.constraints,
        examples: newProblem.examples,
      });
      return response.json();
    },
    onSuccess: (data) => {
      setNewProblem(prev => ({
        ...prev,
        testCases: data.testCases || [],
      }));
      toast({
        title: 'Success',
        description: `Generated ${data.testCases?.length || 0} test cases using AI!`,
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to generate test cases',
        variant: 'destructive',
      });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newProblem.title || !newProblem.description || !newProblem.categoryId) {
      toast({
        title: 'Error',
        description: 'Please fill in all required fields',
        variant: 'destructive',
      });
      return;
    }
    createProblemMutation.mutate(newProblem);
  };

  const handleGenerateTestCases = () => {
    if (!newProblem.title || !newProblem.description) {
      toast({
        title: 'Error',
        description: 'Please provide title and description first',
        variant: 'destructive',
      });
      return;
    }
    generateTestCasesMutation.mutate();
  };

  const handleTagsChange = (value: string) => {
    const tags = value.split(',').map(tag => tag.trim()).filter(tag => tag);
    setNewProblem(prev => ({ ...prev, tags }));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h3 className="text-xl font-semibold text-slate-900 dark:text-white">
          Problem Management
        </h3>
        <Dialog open={isAddingProblem} onOpenChange={setIsAddingProblem}>
          <DialogTrigger asChild>
            <Button className="bg-brand-600 hover:bg-brand-700" data-testid="button-add-problem">
              <i className="fas fa-plus mr-2"></i>
              Add Problem
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Add New Problem</DialogTitle>
            </DialogHeader>
            
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Basic Info */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="title" className="required">Title</Label>
                  <Input
                    id="title"
                    value={newProblem.title}
                    onChange={(e) => setNewProblem(prev => ({ ...prev, title: e.target.value }))}
                    placeholder="Two Sum"
                    required
                    data-testid="input-problem-title"
                  />
                </div>
                
                <div>
                  <Label htmlFor="category">Category</Label>
                  <Select 
                    value={newProblem.categoryId} 
                    onValueChange={(value) => setNewProblem(prev => ({ ...prev, categoryId: value }))}
                  >
                    <SelectTrigger data-testid="select-problem-category">
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      {categories.map((category) => (
                        <SelectItem key={category.id} value={category.id}>
                          {category.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="difficulty">Difficulty</Label>
                  <Select 
                    value={newProblem.difficulty} 
                    onValueChange={(value: 'Easy' | 'Medium' | 'Hard') => setNewProblem(prev => ({ ...prev, difficulty: value }))}
                  >
                    <SelectTrigger data-testid="select-problem-difficulty">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Easy">Easy</SelectItem>
                      <SelectItem value="Medium">Medium</SelectItem>
                      <SelectItem value="Hard">Hard</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                
                <div>
                  <Label htmlFor="tags">Tags (comma-separated)</Label>
                  <Input
                    id="tags"
                    value={newProblem.tags.join(', ')}
                    onChange={(e) => handleTagsChange(e.target.value)}
                    placeholder="Array, Hash Table, Two Pointers"
                    data-testid="input-problem-tags"
                  />
                </div>
                
                <div className="flex items-center space-x-2 pt-6">
                  <Switch
                    id="premium"
                    checked={newProblem.isPremium}
                    onCheckedChange={(checked) => setNewProblem(prev => ({ ...prev, isPremium: checked }))}
                    data-testid="switch-problem-premium"
                  />
                  <Label htmlFor="premium">Premium Only</Label>
                </div>
              </div>

              {/* Description */}
              <div>
                <Label htmlFor="description" className="required">Description</Label>
                <Textarea
                  id="description"
                  value={newProblem.description}
                  onChange={(e) => setNewProblem(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Describe the problem in detail..."
                  rows={6}
                  required
                  data-testid="textarea-problem-description"
                />
              </div>

              {/* Constraints */}
              <div>
                <Label htmlFor="constraints">Constraints</Label>
                <Textarea
                  id="constraints"
                  value={newProblem.constraints}
                  onChange={(e) => setNewProblem(prev => ({ ...prev, constraints: e.target.value }))}
                  placeholder="2 ≤ nums.length ≤ 10⁴&#10;-10⁹ ≤ nums[i] ≤ 10⁹"
                  rows={3}
                  data-testid="textarea-problem-constraints"
                />
              </div>

              {/* Test Cases */}
              <div>
                <div className="flex items-center justify-between mb-3">
                  <Label>Test Cases</Label>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handleGenerateTestCases}
                    disabled={generateTestCasesMutation.isPending}
                    data-testid="button-generate-testcases"
                  >
                    {generateTestCasesMutation.isPending ? (
                      <i className="fas fa-spinner fa-spin mr-2"></i>
                    ) : (
                      <i className="fas fa-robot mr-2"></i>
                    )}
                    AI Generate Tests
                  </Button>
                </div>
                
                {newProblem.testCases.length > 0 && (
                  <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-4">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-sm font-medium">Generated Test Cases</span>
                      <Badge variant="outline">{newProblem.testCases.length} cases</Badge>
                    </div>
                    <div className="text-sm text-slate-600 dark:text-slate-400">
                      Test cases have been generated and will be included with the problem.
                    </div>
                  </div>
                )}
              </div>

              {/* Solution */}
              <div>
                <Label htmlFor="solution">Solution (Optional)</Label>
                <Textarea
                  id="solution"
                  value={newProblem.solution}
                  onChange={(e) => setNewProblem(prev => ({ ...prev, solution: e.target.value }))}
                  placeholder="def twoSum(self, nums, target):&#10;    # Solution code here"
                  rows={8}
                  className="font-mono text-sm"
                  data-testid="textarea-problem-solution"
                />
              </div>

              {/* Submit */}
              <div className="flex justify-end space-x-3 pt-6 border-t">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsAddingProblem(false)}
                  data-testid="button-cancel-problem"
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={createProblemMutation.isPending}
                  className="bg-brand-600 hover:bg-brand-700"
                  data-testid="button-save-problem"
                >
                  {createProblemMutation.isPending ? (
                    <i className="fas fa-spinner fa-spin mr-2"></i>
                  ) : null}
                  Save Problem
                </Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Problems List */}
      <Card>
        <CardHeader>
          <CardTitle>Existing Problems</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="animate-pulse flex items-center justify-between p-4 border border-slate-200 dark:border-slate-700 rounded-lg">
                  <div className="flex-1">
                    <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-1/3 mb-2"></div>
                    <div className="h-3 bg-slate-200 dark:bg-slate-700 rounded w-1/2"></div>
                  </div>
                  <div className="flex space-x-2">
                    <div className="w-16 h-6 bg-slate-200 dark:bg-slate-700 rounded"></div>
                    <div className="w-8 h-8 bg-slate-200 dark:bg-slate-700 rounded"></div>
                  </div>
                </div>
              ))}
            </div>
          ) : (problems as any[])?.length > 0 ? (
            <div className="space-y-4">
              {(problems as any[])?.slice(0, 10).map((problem: any) => (
                <div key={problem.id} className="flex items-center justify-between p-4 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <h4 className="font-medium text-slate-900 dark:text-white">
                        {problem.title}
                      </h4>
                      <Badge className={
                        problem.difficulty === 'Easy' ? 'difficulty-easy' :
                        problem.difficulty === 'Medium' ? 'difficulty-medium' : 'difficulty-hard'
                      }>
                        {problem.difficulty}
                      </Badge>
                      {problem.isPremium && (
                        <Badge variant="outline">
                          <i className="fas fa-crown mr-1 text-amber-500"></i>
                          Premium
                        </Badge>
                      )}
                    </div>
                    <div className="flex items-center space-x-4 text-sm text-slate-600 dark:text-slate-400">
                      <span>{problem.acceptanceRate}% acceptance</span>
                      <span>{problem.totalSubmissions} submissions</span>
                      {Array.isArray(problem.tags) && (
                        <span>{problem.tags.slice(0, 2).join(', ')}</span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Button variant="outline" size="sm">
                      <i className="fas fa-edit mr-2"></i>
                      Edit
                    </Button>
                    <Button variant="outline" size="sm" className="text-error-600 hover:text-error-700">
                      <i className="fas fa-trash"></i>
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <i className="fas fa-code text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
              <h4 className="text-lg font-medium text-slate-900 dark:text-white mb-2">
                No problems yet
              </h4>
              <p className="text-slate-600 dark:text-slate-400">
                Create your first problem to get started.
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
