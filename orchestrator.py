
import os
import subprocess
import requests
import json
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

def fetch_repos_with_static_calls(token):
    """Fetch popular C# repos and count static method call usage in each."""
    headers = {"Authorization": f"token {token}"}
    
    print("🔍 Step 1: Fetching popular C# repositories...")
    
    # First, get all popular C# repositories  
    repo_query = "language:C# -is:archived stars:>10000 size:>10000"
    repo_url = f"https://api.github.com/search/repositories?q={requests.utils.quote(repo_query)}&sort=stars&order=desc&per_page=100"
    
    print(f"🔍 Repository Search URL:")
    print(f"   {repo_url}")
    print(f"🔍 Repository Query: '{repo_query}'")
    print()
    
    response = requests.get(repo_url, headers=headers)
    
    if response.status_code != 200:
        print(f"❌ Repository API Error: {response.text}")
        return []
    
    repo_data = response.json()
    repositories = [item["full_name"] for item in repo_data.get("items", [])]
    
    print(f"📊 Found {len(repositories)} popular C# repositories")
    print(f"🔍 Sample repositories: {repositories[:5]}")
    print()
    
    print("🔍 Step 2: Checking static method call usage in ALL repositories...")
    repo_static_counts = {}
    
    # Define query groups for 3 separate searches to avoid complexity issues
    query_groups = [
        ("DateTime.Now OR DateTime.UtcNow", "Time patterns"),
        ("File.Exists OR Directory.Exists", "Existence checks"), 
        ("Guid.NewGuid", "GUID generation")
    ]
    
    print(f"📊 Checking all {len(repositories)} repositories for static method usage...")
    print(f"🔍 Query groups: {[group[1] for group in query_groups]}")
    print(f"🧠 Using adaptive rate limiting: starts at 5.0s, increases by 1s if rate limited (max 6 increases)")
    print(f"⚡ 3 separate queries per repository to avoid complexity issues")
    print()
    
    # Adaptive rate limiting variables
    sleep_time = 5.0  # Start with 5.0 seconds for more conservative approach
    max_increases = 6  # Maximum number of times we can increase the delay
    increases_used = 0 # Track how many increases we've used
    max_retries = 3   # Maximum retries per repository
    import time
    
    i = 1
    while i <= len(repositories):
        repo_name = repositories[i-1]
        print(f"[{i}/{len(repositories)}] Checking {repo_name} (delay: {sleep_time}s)...")
        
        # Search for static method patterns using 3 separate queries to avoid complexity issues
        # Note: path:*.cs filter doesn't work reliably in GitHub API, 
        # so we search all files and filter client-side if needed
        
        # Group patterns into 3 logical queries to reduce complexity per query
        query_groups = [
            ("DateTime.Now OR DateTime.UtcNow", "Time patterns"),
            ("File.Exists OR Directory.Exists", "Existence checks"), 
            ("Guid.NewGuid", "GUID generation")
        ]
        
        total_static_calls = 0
        query_results = {}
        
        retry_count = 0
        success = False
        
        while retry_count < max_retries and not success:
            # Try all 3 queries and sum the results
            all_queries_success = True
            current_total = 0
            current_results = {}
            
            for query_pattern, query_name in query_groups:
                static_query = f"{query_pattern} repo:{repo_name}"
                static_url = f"https://api.github.com/search/code?q={requests.utils.quote(static_query)}&per_page=1"
                
                response = requests.get(static_url, headers=headers)
                
                if response.status_code == 200:
                    data = response.json()
                    count = data.get("total_count", 0)
                    current_results[query_name] = count
                    current_total += count
                    
                    # Use adaptive sleep time between queries to avoid rate limiting
                    time.sleep(sleep_time)
                    
                elif response.status_code == 403:
                    # Rate limited - will retry all queries
                    all_queries_success = False
                    break
                elif response.status_code == 422:
                    # Query too complex, count as 0 for this pattern
                    current_results[query_name] = 0
                else:
                    # Other error, count as 0 for this pattern
                    current_results[query_name] = 0
            
            if all_queries_success:
                total_static_calls = current_total
                query_results = current_results
                repo_static_counts[repo_name] = total_static_calls
                
                if total_static_calls > 0:
                    print(f"   📊 {total_static_calls} total static method calls found")
                    breakdown = ", ".join([f"{name}: {count}" for name, count in query_results.items() if count > 0])
                    if breakdown:
                        print(f"   📋 Breakdown: {breakdown}")
                else:
                    print(f"   📊 {total_static_calls} static method calls found")
                
                success = True
                
            elif not all_queries_success:
                retry_count += 1
                if increases_used < max_increases:
                    old_sleep_time = sleep_time
                    sleep_time += 1.0  # Increase delay by 1 second
                    increases_used += 1
                    print(f"   ⚠️  Rate limited (attempt {retry_count}/{max_retries})")
                    print(f"   🕐 Increasing delay from {old_sleep_time}s to {sleep_time}s (increase {increases_used}/{max_increases})")
                else:
                    print(f"   ⚠️  Rate limited (attempt {retry_count}/{max_retries}) - already at max delay ({sleep_time}s)")
                
                if retry_count < max_retries:
                    print(f"   🔄 Waiting {sleep_time}s and retrying all queries...")
                    time.sleep(sleep_time)
                    # Reset for retry
                    current_total = 0
                    current_results = {}
                else:
                    print(f"   ❌ Max retries exceeded, skipping {repo_name}")
                    repo_static_counts[repo_name] = 0
                    success = True  # Exit retry loop
        
        print()
        
        # Add delay before next request (only if we successfully processed this repo)
        if success and i < len(repositories):
            time.sleep(sleep_time)
        
        i += 1
    
    # Sort repositories by total static call usage count (descending)
    sorted_repos = sorted(repo_static_counts.items(), key=lambda x: x[1], reverse=True)
    
    print("🏆 ALL repositories sorted by core static method call usage:")
    print("=" * 80)
    for i, (repo, count) in enumerate(sorted_repos, 1):
        status = "✅ HAS core static calls" if count > 0 else "❌ No core static calls"
        print(f"{i:2d}. {repo:<50} {count:4d} matches - {status}")
    print("=" * 80)
    
    # Show summary
    total_searched = len(repo_static_counts)
    repos_with_static_calls = len([count for count in repo_static_counts.values() if count > 0])
    print(f"📊 Summary: Searched {total_searched} repos, {repos_with_static_calls} have core static method calls")
    print(f"🕐 Final delay time: {sleep_time}s (started at 5.0s, used {increases_used}/{max_increases} increases)")
    print()
    
    # Select top 10 repositories with static calls usage for cloning and analysis
    top_10_repos = [repo for repo, count in sorted_repos if count > 0][:10]
    
    print(f"🎯 Selected TOP 10 repositories for cloning and analysis:")
    print("=" * 80)
    for i, repo in enumerate(top_10_repos, 1):
        count = repo_static_counts[repo]
        print(f"{i:2d}. {repo:<50} {count:4d} core static call matches")
    print("=" * 80)
    print(f"📊 Will clone and analyze {len(top_10_repos)} repositories")
    print()
    
    return top_10_repos
    
    # Sort repositories by number of files containing DateTime patterns
    # This is a proxy for "repos with most DateTime usage"
    sorted_repos = sorted(repo_counts.items(), key=lambda x: x[1], reverse=True)
    
    print(f"🏆 Repositories with most DateTime-containing files:")
    for i, (repo, file_count) in enumerate(sorted_repos[:10]):
        print(f"   {i+1}. {repo} - {file_count} files with DateTime patterns")
    print()
    
    # Return top repos (those with most files containing DateTime patterns)
    top_repos = [repo for repo, count in sorted_repos[:num_repos]]

    # Return top repos with most DateTime usage
    top_repos = [repo for repo, count in sorted_repos[:num_repos]]
    
    # If we didn't get enough repos, try repository search as fallback
    if len(top_repos) < num_repos:
        print(f"⚠️  Only found {len(top_repos)} repos from code search, trying repository search fallback...")
        fallback_repos = fetch_popular_csharp_repos(token, num_repos - len(top_repos))
        top_repos.extend(fallback_repos)
        top_repos = top_repos[:num_repos]  # Ensure we don't exceed requested count
    
    print(f"🎯 Selected repositories for analysis:")
    for i, repo in enumerate(top_repos, 1):
        file_count = repo_counts.get(repo, 0)
        print(f"   {i}. {repo} ({file_count} files with DateTime patterns)")
    
    return top_repos

def fetch_popular_csharp_repos(token, num_repos):
    """Fallback: Fetch popular C# repositories that likely contain DateTime usage."""
    headers = {"Authorization": f"token {token}"}
    query = "language:csharp stars:>1000"
    url = f"https://api.github.com/search/repositories?q={requests.utils.quote(query)}&sort=stars&order=desc&per_page={num_repos}"
    
    print(f"🔄 Fallback Repository Search URL:")
    print(f"   {url}")
    
    response = requests.get(url, headers=headers)
    if response.status_code != 200:
        print(f"❌ Fallback API Error: {response.text}")
        return []
    
    data = response.json()
    repos = [item["full_name"] for item in data.get("items", [])]
    print(f"📊 Fallback found {len(repos)} popular C# repositories")
    return repos

def clone_repo(repo_name):
    """Clone a GitHub repo locally into the cloned_repos subdirectory."""
    # Create cloned_repos directory if it doesn't exist
    cloned_repos_dir = "cloned_repos"
    if not os.path.exists(cloned_repos_dir):
        os.makedirs(cloned_repos_dir)
    
    repo_url = f"https://github.com/{repo_name}.git"
    repo_dir_name = repo_name.split("/")[-1]
    repo_path = os.path.join(cloned_repos_dir, repo_dir_name)
    
    if not os.path.exists(repo_path):
        print(f"Cloning {repo_name} into {repo_path}...")
        subprocess.run(["git", "clone", repo_url, repo_path])
    else:
        print(f"Repository {repo_name} already exists at {repo_path}")
    
    return repo_path

def verify_datetime_usage(repo_path):
    """Manually verify if the repo contains DateTime.Now or DateTime.UtcNow usage."""
    print(f"🔍 Manually searching for DateTime usage in {repo_path}...")
    
    # Search for DateTime.Now
    try:
        result_now = subprocess.run([
            "grep", "-r", "--include=*.cs", "DateTime\\.Now", repo_path
        ], capture_output=True, text=True)
        
        result_utcnow = subprocess.run([
            "grep", "-r", "--include=*.cs", "DateTime\\.UtcNow", repo_path
        ], capture_output=True, text=True)
        
        now_count = len(result_now.stdout.splitlines()) if result_now.returncode == 0 else 0
        utcnow_count = len(result_utcnow.stdout.splitlines()) if result_utcnow.returncode == 0 else 0
        
        total_matches = now_count + utcnow_count
        print(f"   Found {now_count} DateTime.Now matches")
        print(f"   Found {utcnow_count} DateTime.UtcNow matches")
        print(f"   Total: {total_matches} DateTime static calls")
        
        if total_matches > 0:
            print(f"   ✅ This repo DOES contain DateTime static calls!")
            # Show a few examples
            if result_now.stdout:
                print(f"   📝 Example DateTime.Now usage:")
                for line in result_now.stdout.splitlines()[:3]:
                    print(f"      {line}")
        else:
            print(f"   ❌ This repo does NOT contain DateTime static calls!")
            
        return total_matches > 0
        
    except Exception as e:
        print(f"   ⚠️ Error searching for DateTime usage: {e}")
        return False

def run_static_call_analyzer(analyzer_path, repo_path):
    """Run the StaticCallAnalyzer .NET project on a given repo."""
    print(f"🔧 Running analyzer command:")
    command = ["dotnet", "run", "--project", os.path.join(analyzer_path, "StaticCallAnalyzer.csproj"), repo_path]
    print(f"   {' '.join(command)}")
    
    result = subprocess.run(command, capture_output=True, text=True)
    
    print(f"📤 Analyzer exit code: {result.returncode}")
    if result.stderr:
        print(f"📤 Analyzer stderr: {result.stderr}")
    print(f"📤 Analyzer output length: {len(result.stdout)} characters")
    if result.stdout:
        print(f"📤 First 200 chars of output: {result.stdout[:200]}...")
    
    return result.stdout

def orchestrate(token, analyzer_path, num_repos=2):
    print(f"Fetching top {num_repos} repositories with static method calls...")
    repos = fetch_repos_with_static_calls(token, num_repos)
    print(f"Found {len(repos)} repositories to analyze:")
    for i, repo in enumerate(repos, 1):
        print(f"  {i}. {repo}")
    print()
    
    all_results = []

    for i, repo in enumerate(repos, 1):
        print(f"[{i}/{len(repos)}] Processing repo: {repo}")
        repo_dir = clone_repo(repo)
        
        # First, manually verify the repo contains DateTime usage
        has_datetime = verify_datetime_usage(repo_dir)
        print()
        
        if not has_datetime:
            print(f"⚠️ Skipping analyzer since no DateTime usage found manually")
            print()
            continue
            
        print(f"🔧 Running analyzer on {repo_dir}...")
        output = run_static_call_analyzer(analyzer_path, repo_dir)

        try:
            results = json.loads(output)
            for r in results:
                r["Repo"] = repo
            all_results.extend(results)
            print(f"✓ Successfully analyzed {repo} - found {len(results)} results")
        except json.JSONDecodeError:
            print(f"✗ Failed to parse analyzer output for repo: {repo}")
        print()

    # Rank by complexity and static call count
    ranked = sorted(all_results, key=lambda x: (x["Complexity"], x["StaticCallCount"]), reverse=True)

    # Save to JSON file
    with open("analysis_results.json", "w") as f:
        json.dump(ranked, f, indent=4)

    print("Analysis complete. Results saved to analysis_results.json")

def main():
    """Main function for debugging and running the orchestrator."""
    # Get GitHub token from environment variable
    token = os.getenv("GITHUB_TOKEN")
    if not token or token == "your_github_token_here":
        print("Error: Please set your GITHUB_TOKEN in the .env file")
        print("1. Edit the .env file and replace 'your_github_token_here' with your actual GitHub token")
        print("2. Get a token from: https://github.com/settings/tokens")
        return
    
    # Path to the StaticCallAnalyzer project
    analyzer_path = "/workspaces/mocking-static-methods/StaticCallAnalyzer"
    
    print("🚀 Starting comprehensive DateTime.Now analysis...")
    print("📋 Plan:")
    print("   1. Get all popular C# repositories (78 repos)")  
    print("   2. Check DateTime.Now usage in each repository")
    print("   3. Sort by DateTime.Now file count (descending)")
    print("   4. Clone TOP 10 repositories with highest usage") 
    print("   5. Run StaticCallAnalyzer on those 10 repositories")
    print(f"   6. Repos will be cloned into: ./cloned_repos/")
    print("-" * 60)
    
    orchestrate(token, analyzer_path)

def orchestrate(token, analyzer_path):
    print(f"Fetching repositories and analyzing static method call usage...")
    repos = fetch_repos_with_static_calls(token)
    
    if not repos:
        print("❌ No repositories found with static method call usage")
        return
        
    all_results = []

    for i, repo in enumerate(repos, 1):
        print(f"[{i}/{len(repos)}] Processing repo: {repo}")
        repo_dir = clone_repo(repo)
        
        # First, manually verify the repo contains DateTime usage
        has_datetime = verify_datetime_usage(repo_dir)
        print()
        
        if not has_datetime:
            print(f"⚠️ Skipping analyzer since no DateTime usage found manually")
            print()
            continue
            
        print(f"🔧 Running analyzer on {repo_dir}...")
        output = run_static_call_analyzer(analyzer_path, repo_dir)

if __name__ == "__main__":
    main()

# Example usage:
# orchestrate("YOUR_GITHUB_TOKEN", "/path/to/StaticCallAnalyzer", num_repos=2)
