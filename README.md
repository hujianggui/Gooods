##Algorithms for recommendation systems in C#.

### The algorithms are listed as below.
	[0] Global mean, user mean, item mean filling methods. Those are used for rating prediction.
	[1] UserKNN (user-based collaborative filstering), rating prediction, top-N recommendation.
	[2] ItemKNN (item-based collaborative filstering), top-N recommendation.
	[3] Matrix Factorization, rating prediction, top-N recommendation.
	[4] Biased Matrix Factorization, rating prediction, top-N recommendation.
	[5] SVD++, rating prediction.
	[6] Alternating Least Lquares, rating prediction, top-N recommendation.
	[7] Euclidean Embedding, rating prediction, top-N recommendation.
	[8] Friend Matrix Factorization, rating prediction, top-N recommendation.
	    Friend Biased Matrix Factorization, rating prediction, top-N recommendation.
	    Adaptive Friend Matrix Factorization, rating prediction, top-N recommendation.  
	    Adaptive Friend Biased Matrix Factorization, rating prediction, top-N recommendation.
		citation:
	    @inproceedings{DBLP:conf/pakdd/WangYHH15,
	      author    = {Zhijin Wang and
	                   Yan Yang and
	                   Qinmin Hu and
	                   Liang He},
	      title     = {An Empirical Study of Personal Factors and Social Effects on Rating Prediction},
	      booktitle = {Proceedings of the 19th Pacific-Asia Conference on Knowledge Discovery and Data Mining},
	      pages     = {747-758},
	      year      = {2015},
	      series    = {PAKDD'15},
	    }

### Evaluation metrics
	[1] error metrics: MAE, RMSE
	[2] accuracy metrics: Precsion@N, Recall@N
	[3] Rank metrics: MAP@N

### Data collections (examples)
	[1] ml-100k, ml-1m, ml-10m
	[2] Douban
	[3] Epinions
	[4] Flixster
	[5] Yelp

### Contact
	freepose@126.com
