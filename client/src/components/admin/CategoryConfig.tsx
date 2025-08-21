import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { apiRequest } from '@/lib/queryClient';
import { Category } from '@shared/schema';

export function CategoryConfig() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [isAddingCategory, setIsAddingCategory] = useState(false);
  const [newCategory, setNewCategory] = useState({
    name: '',
    slug: '',
    description: '',
    freeProblemLimit: 10,
  });

  const { data: categories = [], isLoading } = useQuery<Category[]>({
    queryKey: ['/api/categories'],
  });

  const updateCategoryMutation = useMutation({
    mutationFn: async ({ id, updates }: { id: string; updates: Partial<Category> }) => {
      const response = await apiRequest('PUT', `/api/categories/${id}`, updates);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Category updated successfully!',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/categories'] });
      setEditingCategory(null);
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update category',
        variant: 'destructive',
      });
    },
  });

  const createCategoryMutation = useMutation({
    mutationFn: async (categoryData: any) => {
      const response = await apiRequest('POST', '/api/categories', categoryData);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Category created successfully!',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/categories'] });
      setIsAddingCategory(false);
      setNewCategory({
        name: '',
        slug: '',
        description: '',
        freeProblemLimit: 10,
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create category',
        variant: 'destructive',
      });
    },
  });

  const handleFreeLimitChange = (categoryId: string, newLimit: number) => {
    updateCategoryMutation.mutate({
      id: categoryId,
      updates: { freeProblemLimit: newLimit },
    });
  };

  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-|-$/g, '');
  };

  const handleNameChange = (value: string) => {
    setNewCategory(prev => ({
      ...prev,
      name: value,
      slug: generateSlug(value),
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategory.name || !newCategory.slug) {
      toast({
        title: 'Error',
        description: 'Please fill in all required fields',
        variant: 'destructive',
      });
      return;
    }
    createCategoryMutation.mutate(newCategory);
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[...Array(6)].map((_, i) => (
          <Card key={i} className="animate-pulse">
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="h-5 bg-slate-200 dark:bg-slate-700 rounded w-1/3 mb-2"></div>
                  <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-1/2"></div>
                </div>
                <div className="flex items-center space-x-4">
                  <div className="w-20 h-8 bg-slate-200 dark:bg-slate-700 rounded"></div>
                  <div className="w-8 h-8 bg-slate-200 dark:bg-slate-700 rounded"></div>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h3 className="text-xl font-semibold text-slate-900 dark:text-white">
            Category Configuration
          </h3>
          <p className="text-slate-600 dark:text-slate-400 mt-1">
            Configure how many problems in each category are free vs premium
          </p>
        </div>
        
        <Dialog open={isAddingCategory} onOpenChange={setIsAddingCategory}>
          <DialogTrigger asChild>
            <Button className="bg-brand-600 hover:bg-brand-700" data-testid="button-add-category">
              <i className="fas fa-plus mr-2"></i>
              Add Category
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Add New Category</DialogTitle>
            </DialogHeader>
            
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <Label htmlFor="name" className="required">Name</Label>
                <Input
                  id="name"
                  value={newCategory.name}
                  onChange={(e) => handleNameChange(e.target.value)}
                  placeholder="Arrays & Hashing"
                  required
                  data-testid="input-category-name"
                />
              </div>
              
              <div>
                <Label htmlFor="slug">Slug</Label>
                <Input
                  id="slug"
                  value={newCategory.slug}
                  onChange={(e) => setNewCategory(prev => ({ ...prev, slug: e.target.value }))}
                  placeholder="arrays-hashing"
                  data-testid="input-category-slug"
                />
                <p className="text-xs text-slate-500 mt-1">
                  Auto-generated from name. Used in URLs.
                </p>
              </div>
              
              <div>
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={newCategory.description}
                  onChange={(e) => setNewCategory(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Array manipulation and hash table problems"
                  data-testid="input-category-description"
                />
              </div>
              
              <div>
                <Label htmlFor="freeProblemLimit">Free Problems Limit</Label>
                <Input
                  id="freeProblemLimit"
                  type="number"
                  min="0"
                  value={newCategory.freeProblemLimit}
                  onChange={(e) => setNewCategory(prev => ({ ...prev, freeProblemLimit: parseInt(e.target.value) || 0 }))}
                  data-testid="input-category-free-limit"
                />
                <p className="text-xs text-slate-500 mt-1">
                  Number of problems that can be accessed for free in this category
                </p>
              </div>
              
              <div className="flex justify-end space-x-3 pt-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsAddingCategory(false)}
                  data-testid="button-cancel-category"
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={createCategoryMutation.isPending}
                  className="bg-brand-600 hover:bg-brand-700"
                  data-testid="button-save-category"
                >
                  {createCategoryMutation.isPending ? (
                    <i className="fas fa-spinner fa-spin mr-2"></i>
                  ) : null}
                  Create Category
                </Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Categories List */}
      <div className="space-y-4">
        {categories.map((category) => (
          <Card key={category.id}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h4 className="text-lg font-medium text-slate-900 dark:text-white">
                      {category.name}
                    </h4>
                    <Badge variant="outline">
                      {category.totalProblems} total problems
                    </Badge>
                  </div>
                  <p className="text-sm text-slate-600 dark:text-slate-400 mb-3">
                    {category.description}
                  </p>
                  
                  {/* Progress Bar */}
                  <div className="flex items-center space-x-3">
                    <div className="flex-1">
                      <div className="flex items-center justify-between text-xs text-slate-600 dark:text-slate-400 mb-1">
                        <span>Free: {category.freeProblemLimit}</span>
                        <span>Premium: {Math.max(0, category.totalProblems - category.freeProblemLimit)}</span>
                      </div>
                      <div className="w-full bg-slate-200 dark:bg-slate-700 rounded-full h-2">
                        <div
                          className="bg-success-600 h-2 rounded-full transition-all"
                          style={{
                            width: `${category.totalProblems > 0 ? (category.freeProblemLimit / category.totalProblems) * 100 : 0}%`
                          }}
                        ></div>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div className="flex items-center space-x-4 ml-6">
                  {/* Free Limit Input */}
                  <div className="flex items-center space-x-2">
                    <Label htmlFor={`limit-${category.id}`} className="text-sm whitespace-nowrap">
                      Free problems:
                    </Label>
                    <Input
                      id={`limit-${category.id}`}
                      type="number"
                      min="0"
                      max={category.totalProblems}
                      value={editingCategory?.id === category.id ? editingCategory.freeProblemLimit : category.freeProblemLimit}
                      onChange={(e) => {
                        const newLimit = parseInt(e.target.value) || 0;
                        if (editingCategory?.id === category.id) {
                          setEditingCategory({ ...editingCategory, freeProblemLimit: newLimit });
                        } else {
                          setEditingCategory({ ...category, freeProblemLimit: newLimit });
                        }
                      }}
                      onBlur={() => {
                        if (editingCategory?.id === category.id && editingCategory.freeProblemLimit !== category.freeProblemLimit) {
                          handleFreeLimitChange(category.id, editingCategory.freeProblemLimit);
                        }
                      }}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' && editingCategory?.id === category.id) {
                          handleFreeLimitChange(category.id, editingCategory.freeProblemLimit);
                        }
                      }}
                      className="w-20"
                      data-testid={`input-free-limit-${category.slug}`}
                    />
                  </div>
                  
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      if (editingCategory?.id === category.id) {
                        handleFreeLimitChange(category.id, editingCategory.freeProblemLimit);
                      }
                    }}
                    disabled={updateCategoryMutation.isPending}
                    data-testid={`button-save-${category.slug}`}
                  >
                    {updateCategoryMutation.isPending ? (
                      <i className="fas fa-spinner fa-spin"></i>
                    ) : (
                      <i className="fas fa-save"></i>
                    )}
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {categories.length === 0 && (
        <Card>
          <CardContent className="p-12 text-center">
            <i className="fas fa-folder text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
            <h4 className="text-lg font-medium text-slate-900 dark:text-white mb-2">
              No categories yet
            </h4>
            <p className="text-slate-600 dark:text-slate-400">
              Create your first category to organize problems.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
