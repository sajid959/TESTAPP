import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Category } from '@/types/api';

interface ProblemFiltersProps {
  categories: Category[];
  filters: {
    categoryId?: string;
    difficulty?: string;
    isPaid?: boolean;
    search?: string;
  };
  onFiltersChange: (filters: any) => void;
}

export function ProblemFilters({ categories, filters, onFiltersChange }: ProblemFiltersProps) {
  const [searchValue, setSearchValue] = useState(filters.search || '');

  const handleFilterChange = (key: string, value: any) => {
    const newFilters = { ...filters };
    
    if (value === 'all' || value === '' || value === undefined) {
      delete newFilters[key as keyof typeof newFilters];
    } else {
      (newFilters as any)[key] = value;
    }
    
    onFiltersChange(newFilters);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchValue(value);
    
    // Debounce search
    const timeoutId = setTimeout(() => {
      handleFilterChange('search', value || undefined);
    }, 300);
    
    return () => clearTimeout(timeoutId);
  };

  const clearFilters = () => {
    setSearchValue('');
    onFiltersChange({});
  };

  const activeFiltersCount = Object.keys(filters).length;

  return (
    <Card className="mb-6">
      <CardContent className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
            Filter Problems
          </h3>
          {activeFiltersCount > 0 && (
            <div className="flex items-center space-x-2">
              <Badge variant="outline">{activeFiltersCount} filter(s) active</Badge>
              <Button 
                variant="ghost" 
                size="sm" 
                onClick={clearFilters}
                data-testid="button-clear-filters"
              >
                Clear All
              </Button>
            </div>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Search */}
          <div>
            <Label htmlFor="search" className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2 block">
              Search
            </Label>
            <div className="relative">
              <Input
                id="search"
                type="text"
                placeholder="Search problems..."
                value={searchValue}
                onChange={handleSearchChange}
                className="pl-10"
                data-testid="input-search-problems"
              />
              <i className="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400"></i>
            </div>
          </div>

          {/* Category */}
          <div>
            <Label htmlFor="category" className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2 block">
              Category
            </Label>
            <Select 
              value={filters.categoryId || 'all'} 
              onValueChange={(value) => handleFilterChange('categoryId', value)}
            >
              <SelectTrigger data-testid="select-category">
                <SelectValue placeholder="All Categories" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Categories</SelectItem>
                {categories.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Difficulty */}
          <div>
            <Label htmlFor="difficulty" className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2 block">
              Difficulty
            </Label>
            <Select 
              value={filters.difficulty || 'all'} 
              onValueChange={(value) => handleFilterChange('difficulty', value)}
            >
              <SelectTrigger data-testid="select-difficulty">
                <SelectValue placeholder="All Levels" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Levels</SelectItem>
                <SelectItem value="Easy">Easy</SelectItem>
                <SelectItem value="Medium">Medium</SelectItem>
                <SelectItem value="Hard">Hard</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Access Level */}
          <div>
            <Label htmlFor="access" className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-2 block">
              Access
            </Label>
            <Select 
              value={
                filters.isPremium === undefined ? 'all' : 
                filters.isPremium ? 'premium' : 'free'
              } 
              onValueChange={(value) => {
                if (value === 'all') {
                  handleFilterChange('isPremium', undefined);
                } else {
                  handleFilterChange('isPremium', value === 'premium');
                }
              }}
            >
              <SelectTrigger data-testid="select-access">
                <SelectValue placeholder="All Access" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Access</SelectItem>
                <SelectItem value="free">Free</SelectItem>
                <SelectItem value="premium">Premium</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Quick Filters */}
        <div className="mt-4 pt-4 border-t border-slate-200 dark:border-slate-700">
          <Label className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-3 block">
            Quick Filters
          </Label>
          <div className="flex flex-wrap gap-2">
            <Button
              variant={filters.difficulty === 'Easy' ? 'default' : 'outline'}
              size="sm"
              onClick={() => handleFilterChange('difficulty', filters.difficulty === 'Easy' ? undefined : 'Easy')}
              className="h-8"
              data-testid="button-filter-easy"
            >
              Easy Problems
            </Button>
            <Button
              variant={filters.isPremium === false ? 'default' : 'outline'}
              size="sm"
              onClick={() => handleFilterChange('isPremium', filters.isPremium === false ? undefined : false)}
              className="h-8"
              data-testid="button-filter-free"
            >
              Free Only
            </Button>
            <Button
              variant={filters.isPremium === true ? 'default' : 'outline'}
              size="sm"
              onClick={() => handleFilterChange('isPremium', filters.isPremium === true ? undefined : true)}
              className="h-8"
              data-testid="button-filter-premium"
            >
              Premium Only
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
