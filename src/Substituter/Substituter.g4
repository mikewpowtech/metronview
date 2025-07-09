grammar Substituter;

/*
 * Parser Rules
 */

compileUnit returns [ChunkList chunkList]
	:	{$chunkList = new ChunkList(); } (chunk { $chunkList.Chunks.Add($chunk.retval); })* EOF
	;

chunk returns [Chunk retval]
	: sub { $retval = new Chunk(true, $sub.retval); }
	| nonSub { $retval = new Chunk(false, $nonSub.retval); }
	;

sub returns [string retval]
	: LEFTBRACE NOTBRACE RIGHTBRACE { $retval = $NOTBRACE.text; }
	;

nonSub returns [string retval]
	: NOTBRACE { $retval = $NOTBRACE.text; }
	;

/*
 * Lexer Rules
 */

LEFTBRACE: '{';
RIGHTBRACE: '}';

NOTBRACE: ~[{}]+;
