import { useEffect, useRef, useState } from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

interface MonacoEditorProps {
  value: string;
  onChange: (value: string) => void;
  language: string;
  onLanguageChange: (language: string) => void;
  onRun: () => void;
  onSubmit: () => void;
  isRunning?: boolean;
  isSubmitting?: boolean;
}

const languages = [
  { id: 'python', name: 'Python3', extension: 'py' },
  { id: 'javascript', name: 'JavaScript', extension: 'js' },
  { id: 'typescript', name: 'TypeScript', extension: 'ts' },
  { id: 'java', name: 'Java', extension: 'java' },
  { id: 'cpp', name: 'C++', extension: 'cpp' },
  { id: 'c', name: 'C', extension: 'c' },
  { id: 'csharp', name: 'C#', extension: 'cs' },
  { id: 'php', name: 'PHP', extension: 'php' },
  { id: 'go', name: 'Go', extension: 'go' },
  { id: 'rust', name: 'Rust', extension: 'rs' },
];

const defaultCode: Record<string, string> = {
  python: `class Solution:
    def twoSum(self, nums: List[int], target: int) -> List[int]:
        # TODO: Implement your solution
        hashmap = {}
        
        for i, num in enumerate(nums):
            complement = target - num
            if complement in hashmap:
                return [hashmap[complement], i]
            hashmap[num] = i`,
  javascript: `/**
 * @param {number[]} nums
 * @param {number} target
 * @return {number[]}
 */
var twoSum = function(nums, target) {
    // TODO: Implement your solution
    const hashmap = {};
    
    for (let i = 0; i < nums.length; i++) {
        const complement = target - nums[i];
        if (complement in hashmap) {
            return [hashmap[complement], i];
        }
        hashmap[nums[i]] = i;
    }
};`,
  java: `class Solution {
    public int[] twoSum(int[] nums, int target) {
        // TODO: Implement your solution
        Map<Integer, Integer> hashmap = new HashMap<>();
        
        for (int i = 0; i < nums.length; i++) {
            int complement = target - nums[i];
            if (hashmap.containsKey(complement)) {
                return new int[] { hashmap.get(complement), i };
            }
            hashmap.put(nums[i], i);
        }
        
        return new int[0];
    }
}`,
  cpp: `class Solution {
public:
    vector<int> twoSum(vector<int>& nums, int target) {
        // TODO: Implement your solution
        unordered_map<int, int> hashmap;
        
        for (int i = 0; i < nums.size(); i++) {
            int complement = target - nums[i];
            if (hashmap.find(complement) != hashmap.end()) {
                return {hashmap[complement], i};
            }
            hashmap[nums[i]] = i;
        }
        
        return {};
    }
};`,
  php: `<?php
class Solution {
    /**
     * @param Integer[] $nums
     * @param Integer $target
     * @return Integer[]
     */
    function twoSum($nums, $target) {
        // TODO: Implement your solution
        $hashmap = array();
        
        for ($i = 0; $i < count($nums); $i++) {
            $complement = $target - $nums[$i];
            if (array_key_exists($complement, $hashmap)) {
                return array($hashmap[$complement], $i);
            }
            $hashmap[$nums[$i]] = $i;
        }
        
        return array();
    }
}
?>`,
  c: `#include <stdio.h>
#include <stdlib.h>

int* twoSum(int* nums, int numsSize, int target, int* returnSize){
    // TODO: Implement your solution
    *returnSize = 2;
    int* result = (int*)malloc(2 * sizeof(int));
    
    for (int i = 0; i < numsSize - 1; i++) {
        for (int j = i + 1; j < numsSize; j++) {
            if (nums[i] + nums[j] == target) {
                result[0] = i;
                result[1] = j;
                return result;
            }
        }
    }
    
    *returnSize = 0;
    return result;
}`,
  csharp: `public class Solution {
    public int[] TwoSum(int[] nums, int target) {
        // TODO: Implement your solution
        Dictionary<int, int> hashmap = new Dictionary<int, int>();
        
        for (int i = 0; i < nums.Length; i++) {
            int complement = target - nums[i];
            if (hashmap.ContainsKey(complement)) {
                return new int[] { hashmap[complement], i };
            }
            hashmap[nums[i]] = i;
        }
        
        return new int[0];
    }
}`,
  go: `func twoSum(nums []int, target int) []int {
    // TODO: Implement your solution
    hashmap := make(map[int]int)
    
    for i, num := range nums {
        complement := target - num
        if index, exists := hashmap[complement]; exists {
            return []int{index, i}
        }
        hashmap[num] = i
    }
    
    return []int{}
}`,
  rust: `impl Solution {
    pub fn two_sum(nums: Vec<i32>, target: i32) -> Vec<i32> {
        // TODO: Implement your solution
        use std::collections::HashMap;
        let mut hashmap = HashMap::new();
        
        for (i, &num) in nums.iter().enumerate() {
            let complement = target - num;
            if let Some(&index) = hashmap.get(&complement) {
                return vec![index as i32, i as i32];
            }
            hashmap.insert(num, i);
        }
        
        vec![]
    }
}`,
  typescript: `function twoSum(nums: number[], target: number): number[] {
    // TODO: Implement your solution
    const hashmap: {[key: number]: number} = {};
    
    for (let i = 0; i < nums.length; i++) {
        const complement = target - nums[i];
        if (complement in hashmap) {
            return [hashmap[complement], i];
        }
        hashmap[nums[i]] = i;
    }
    
    return [];
}`
};

export function MonacoEditor({
  value,
  onChange,
  language,
  onLanguageChange,
  onRun,
  onSubmit,
  isRunning = false,
  isSubmitting = false
}: MonacoEditorProps) {
  const editorRef = useRef<HTMLDivElement>(null);
  const monacoInstance = useRef<any>(null);
  const [isLoaded, setIsLoaded] = useState(false);
  const [lastSaved, setLastSaved] = useState<Date | null>(null);

  useEffect(() => {
    if (typeof window !== 'undefined' && (window as any).require) {
      (window as any).require(['vs/editor/editor.main'], () => {
        initializeEditor();
      });
    } else {
      // Load Monaco dynamically
      const script = document.createElement('script');
      script.src = 'https://unpkg.com/monaco-editor@0.44.0/min/vs/loader.js';
      script.onload = () => {
        (window as any).require.config({
          paths: { vs: 'https://unpkg.com/monaco-editor@0.44.0/min/vs' }
        });
        (window as any).require(['vs/editor/editor.main'], () => {
          initializeEditor();
        });
      };
      document.head.appendChild(script);
    }

    return () => {
      if (monacoInstance.current) {
        monacoInstance.current.dispose();
      }
    };
  }, []);

  const initializeEditor = () => {
    if (editorRef.current && !monacoInstance.current) {
      const monaco = (window as any).monaco;
      
      // Configure theme
      monaco.editor.defineTheme('dsagrind-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
          'editor.background': '#1e1e1e',
          'editor.foreground': '#d4d4d4',
        }
      });

      monacoInstance.current = monaco.editor.create(editorRef.current, {
        value: value || defaultCode[language] || '',
        language: language === 'cpp' ? 'cpp' : language === 'csharp' ? 'csharp' : language,
        theme: 'dsagrind-dark',
        fontSize: 14,
        fontFamily: 'JetBrains Mono, Consolas, Monaco, monospace',
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        automaticLayout: true,
        tabSize: 4,
        insertSpaces: true,
        wordWrap: 'on',
        lineNumbers: 'on',
        renderLineHighlight: 'line',
        selectOnLineNumbers: true,
      });

      monacoInstance.current.onDidChangeModelContent(() => {
        const newValue = monacoInstance.current.getValue();
        onChange(newValue);
        setLastSaved(new Date());
      });

      // Keyboard shortcuts
      monacoInstance.current.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, onRun);
      monacoInstance.current.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Enter, onSubmit);

      setIsLoaded(true);
    }
  };

  useEffect(() => {
    if (monacoInstance.current && value !== monacoInstance.current.getValue()) {
      monacoInstance.current.setValue(value || defaultCode[language] || '');
    }
  }, [value, language]);

  useEffect(() => {
    if (monacoInstance.current) {
      const monaco = (window as any).monaco;
      const model = monacoInstance.current.getModel();
      const monacoLanguage = language === 'cpp' ? 'cpp' : 
                          language === 'csharp' ? 'csharp' : 
                          language === 'php' ? 'php' : language;
      monaco.editor.setModelLanguage(model, monacoLanguage);
    }
  }, [language]);

  const handleLanguageChange = (newLanguage: string) => {
    onLanguageChange(newLanguage);
    if (monacoInstance.current) {
      monacoInstance.current.setValue(defaultCode[newLanguage] || '');
    }
  };

  return (
    <div className="flex flex-col h-full">
      {/* Editor Header */}
      <div className="bg-slate-50 dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700 px-6 py-3 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Select value={language} onValueChange={handleLanguageChange}>
            <SelectTrigger className="w-[180px]" data-testid="select-language">
              <SelectValue placeholder="Select language" />
            </SelectTrigger>
            <SelectContent>
              {languages.map((lang) => (
                <SelectItem key={lang.id} value={lang.id}>
                  {lang.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          <div className="flex items-center space-x-3">
            <Button
              variant="outline"
              size="sm"
              onClick={onRun}
              disabled={isRunning}
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
              size="sm"
              onClick={onSubmit}
              disabled={isSubmitting}
              className="bg-brand-600 hover:bg-brand-700"
              data-testid="button-submit-code"
            >
              {isSubmitting ? (
                <i className="fas fa-spinner fa-spin mr-2"></i>
              ) : (
                <i className="fas fa-check mr-2"></i>
              )}
              Submit
            </Button>
          </div>
        </div>
        
        <div className="flex items-center space-x-2 text-sm text-slate-600 dark:text-slate-400">
          {lastSaved && (
            <>
              <span>Auto-saved {lastSaved.toLocaleTimeString()}</span>
              <i className="fas fa-cloud-upload-alt text-success-500"></i>
            </>
          )}
          <Badge variant="outline" className="text-xs">
            Ctrl+Enter to Run • Ctrl+Shift+Enter to Submit
          </Badge>
        </div>
      </div>

      {/* Editor */}
      <div className="flex-1 relative">
        <div
          ref={editorRef}
          className="w-full h-full code-editor"
          data-testid="monaco-editor"
        />
        
        {!isLoaded && (
          <div className="absolute inset-0 bg-slate-900 flex items-center justify-center">
            <div className="text-center">
              <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
              <p className="text-slate-400">Loading editor...</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
