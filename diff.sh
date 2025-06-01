git diff --name-only | sort | awk 'BEGIN {FS="/"} {
  path="";
  for (i=1; i<=NF; i++) {
    path = (path ? path "/" : "") $i;
    if (!seen[path]) {
      seen[path]=1;
      indent = "  ";
      print indent^(i-1) path;
    }
  }
}' > unstaged_tree.txt
