import { useState, useRef } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/hooks/use-toast';
import { apiRequest } from '@/lib/queryClient';

interface ImportResult {
  total: number;
  imported: number;
  errors: Array<{
    row: number;
    error: string;
    data: any;
  }>;
}

export function BulkImport() {
  const { toast } = useToast();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      setIsUploading(true);
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await fetch('/api/admin/bulk-import', {
        method: 'POST',
        body: formData,
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
        },
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Upload failed');
      }
      
      return response.json();
    },
    onSuccess: (result: ImportResult) => {
      setImportResult(result);
      toast({
        title: 'Import Complete',
        description: `Successfully imported ${result.imported} out of ${result.total} problems.`,
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Import Failed',
        description: error.message || 'Failed to import problems',
        variant: 'destructive',
      });
    },
    onSettled: () => {
      setIsUploading(false);
    },
  });

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      const allowedTypes = [
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
        'application/vnd.ms-excel', // .xls
        'text/csv', // .csv
      ];
      
      if (!allowedTypes.includes(file.type)) {
        toast({
          title: 'Invalid File Type',
          description: 'Please select an Excel (.xlsx, .xls) or CSV file.',
          variant: 'destructive',
        });
        return;
      }
      
      setSelectedFile(file);
      setImportResult(null);
    }
  };

  const handleUpload = () => {
    if (!selectedFile) {
      toast({
        title: 'No File Selected',
        description: 'Please select a file to upload.',
        variant: 'destructive',
      });
      return;
    }
    
    uploadMutation.mutate(selectedFile);
  };

  const resetUpload = () => {
    setSelectedFile(null);
    setImportResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const downloadTemplate = () => {
    // Create a sample CSV template
    const headers = [
      'title',
      'description',
      'difficulty',
      'categorySlug',
      'tags',
      'isPremium',
      'constraints',
      'examples',
      'testCases',
      'solution',
      'hints'
    ];
    
    const sampleRow = [
      'Two Sum',
      'Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.',
      'Easy',
      'arrays-hashing',
      'Array,Hash Table',
      'false',
      '2 ≤ nums.length ≤ 10⁴',
      '[{"input":"nums = [2,7,11,15], target = 9","output":"[0,1]","explanation":"Because nums[0] + nums[1] == 9, we return [0, 1]."}]',
      '[{"input":{"nums":[2,7,11,15],"target":9},"output":[0,1]}]',
      'def twoSum(self, nums, target):\n    hashmap = {}\n    for i, num in enumerate(nums):\n        complement = target - num\n        if complement in hashmap:\n            return [hashmap[complement], i]\n        hashmap[num] = i',
      '["Think about using a hash map","Store numbers you\'ve seen with their indices"]'
    ];
    
    const csvContent = [headers, sampleRow]
      .map(row => row.map(field => `"${field}"`).join(','))
      .join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'problems-template.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h3 className="text-xl font-semibold text-slate-900 dark:text-white">
            Bulk Import Problems
          </h3>
          <p className="text-slate-600 dark:text-slate-400 mt-1">
            Upload Excel or CSV files to import multiple problems at once
          </p>
        </div>
        
        <Button
          variant="outline"
          onClick={downloadTemplate}
          data-testid="button-download-template"
        >
          <i className="fas fa-download mr-2"></i>
          Download Template
        </Button>
      </div>

      <Tabs defaultValue="upload" className="space-y-6">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="upload">Upload File</TabsTrigger>
          <TabsTrigger value="format">Format Guide</TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="space-y-6">
          {/* Upload Section */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <i className="fas fa-upload mr-2"></i>
                File Upload
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* File Selection */}
              <div>
                <Label htmlFor="file">Select File</Label>
                <div className="mt-2">
                  <Input
                    ref={fileInputRef}
                    id="file"
                    type="file"
                    accept=".xlsx,.xls,.csv"
                    onChange={handleFileSelect}
                    disabled={isUploading}
                    data-testid="input-bulk-import-file"
                  />
                  <p className="text-sm text-slate-500 mt-2">
                    Supported formats: Excel (.xlsx, .xls) and CSV files
                  </p>
                </div>
              </div>

              {/* Selected File Info */}
              {selectedFile && (
                <Alert>
                  <i className="fas fa-file"></i>
                  <AlertDescription>
                    <div className="flex items-center justify-between">
                      <div>
                        <strong>{selectedFile.name}</strong>
                        <span className="ml-2 text-slate-500">
                          ({(selectedFile.size / 1024).toFixed(1)} KB)
                        </span>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={resetUpload}
                        disabled={isUploading}
                        data-testid="button-reset-upload"
                      >
                        <i className="fas fa-times"></i>
                      </Button>
                    </div>
                  </AlertDescription>
                </Alert>
              )}

              {/* Upload Progress */}
              {isUploading && (
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium">Importing problems...</span>
                    <span className="text-sm text-slate-500">Processing with AI</span>
                  </div>
                  <Progress value={75} className="w-full" />
                  <p className="text-xs text-slate-500">
                    Using AI to fill in missing data and generate test cases...
                  </p>
                </div>
              )}

              {/* Upload Button */}
              <Button
                onClick={handleUpload}
                disabled={!selectedFile || isUploading}
                className="w-full bg-brand-600 hover:bg-brand-700"
                data-testid="button-start-import"
              >
                {isUploading ? (
                  <>
                    <i className="fas fa-spinner fa-spin mr-2"></i>
                    Importing...
                  </>
                ) : (
                  <>
                    <i className="fas fa-upload mr-2"></i>
                    Start Import
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Import Results */}
          {importResult && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center">
                  <i className="fas fa-chart-bar mr-2"></i>
                  Import Results
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                  <div className="text-center p-4 bg-slate-50 dark:bg-slate-800 rounded-lg">
                    <div className="text-2xl font-bold text-slate-900 dark:text-white">
                      {importResult.total}
                    </div>
                    <div className="text-sm text-slate-600 dark:text-slate-400">Total Rows</div>
                  </div>
                  
                  <div className="text-center p-4 bg-success-50 dark:bg-success-900/20 rounded-lg">
                    <div className="text-2xl font-bold text-success-700 dark:text-success-400">
                      {importResult.imported}
                    </div>
                    <div className="text-sm text-success-600 dark:text-success-400">Successfully Imported</div>
                  </div>
                  
                  <div className="text-center p-4 bg-error-50 dark:bg-error-900/20 rounded-lg">
                    <div className="text-2xl font-bold text-error-700 dark:text-error-400">
                      {importResult.errors.length}
                    </div>
                    <div className="text-sm text-error-600 dark:text-error-400">Errors</div>
                  </div>
                </div>

                {/* Error Details */}
                {importResult.errors.length > 0 && (
                  <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white mb-3">
                      Import Errors
                    </h4>
                    <ScrollArea className="h-64 border border-slate-200 dark:border-slate-700 rounded-lg">
                      <div className="p-4 space-y-3">
                        {importResult.errors.map((error, index) => (
                          <div key={index} className="border-l-4 border-error-500 pl-4 py-2">
                            <div className="flex items-center justify-between mb-1">
                              <Badge variant="destructive">Row {error.row}</Badge>
                            </div>
                            <p className="text-sm text-error-700 dark:text-error-400 mb-2">
                              {error.error}
                            </p>
                            <details className="text-xs">
                              <summary className="cursor-pointer text-slate-500 hover:text-slate-700">
                                View row data
                              </summary>
                              <pre className="mt-2 p-2 bg-slate-100 dark:bg-slate-800 rounded text-xs overflow-x-auto">
                                {JSON.stringify(error.data, null, 2)}
                              </pre>
                            </details>
                          </div>
                        ))}
                      </div>
                    </ScrollArea>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="format" className="space-y-6">
          {/* Format Guide */}
          <Card>
            <CardHeader>
              <CardTitle>File Format Requirements</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <Alert>
                <i className="fas fa-info-circle"></i>
                <AlertDescription>
                  Your file must include these required columns. Missing data will be filled automatically using AI.
                </AlertDescription>
              </Alert>

              <div className="space-y-4">
                <h4 className="font-semibold text-slate-900 dark:text-white">Required Columns:</h4>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-3">
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">title</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Problem title (required)</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Two Sum</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">description</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Problem description (AI fillable)</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Given an array...</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">difficulty</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Easy, Medium, or Hard (AI fillable)</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Easy</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">categorySlug</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Category identifier</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">arrays-hashing</code>
                    </div>
                  </div>
                  
                  <div className="space-y-3">
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">tags</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Comma-separated tags</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Array,Hash Table</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">isPremium</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">true or false</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">false</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">testCases</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">JSON array (AI generated if empty)</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">[{`"input": "...", "output": "..."`}]</code>
                    </div>
                    
                    <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-3">
                      <h5 className="font-medium text-slate-900 dark:text-white">solution</h5>
                      <p className="text-sm text-slate-600 dark:text-slate-400">Optional solution code</p>
                      <code className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">def twoSum...</code>
                    </div>
                  </div>
                </div>
              </div>

              <Alert className="bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800">
                <i className="fas fa-robot text-blue-600"></i>
                <AlertDescription>
                  <strong>AI-Powered Import:</strong> Missing descriptions, difficulties, tags, and test cases 
                  will be automatically generated using Perplexity AI. This ensures high-quality, complete problem data.
                </AlertDescription>
              </Alert>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
