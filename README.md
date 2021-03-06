## Algorithms for recommendation systems in C#

### Collaborative filtering
	[0] Global mean, user mean, item mean filling methods. Those are used for rating prediction.
	[1] UserKNN (user-based collaborative filtering), rating prediction.
	    UserKNNv2 (user-based collaborative filtering, multithread for W), top-N recommendation.

        @article{journals/kbs/WangH16,
          author    = {Zhijin Wang and Liang He},
          title     = {User identification for enhancing {IP-TV} recommendation},
          journal   = {Knowledge Based System},
          volume    = {98},
          pages     = {68-75},
          year      = {2016},
        }

	[2] ItemKNNv2 (item-based collaborative filtering, multithread for W), top-N recommendation.
	[3] PageRank (Restart with Random Walk), top-N recommendation.
	[4] Matrix Factorization, rating prediction, top-N recommendation.
	[5] Biased Matrix Factorization, rating prediction, top-N recommendation.
	[6] SVD++ (speed up), rating prediction.
	[7] Alternating Least Lquares, rating prediction, top-N recommendation.
	[8] WRMF (weighted regularized matrix factorization), top-N recommendation.
	[9] Euclidean Embedding, rating prediction, top-N recommendation.
	[10] BPRMF (BPR: Bayesian Personalized Ranking from Implicit Feedback), top-N recommendation.
	[11] SLIM (Sparse LInear Model) (train with multithread), top-N recommendation. 
	[12] FISM (Factored Item Similarity Models), top-N recommendation. This includes:
        FISMrmse which uses an element-wise update for a SE loss function.
        FISMauc which uses a pair-wise update for a AUC loss function.
	[13] SocialMF, rating prediction, top-N recommendation.
	[14] Friend Matrix Factorization, rating prediction, top-N recommendation.
	    Friend Biased Matrix Factorization, rating prediction, top-N recommendation.
		
	    @inproceedings{conf/pakdd/WangYHH15,
	      author    = {Zhijin Wang and Yan Yang and Qinmin Hu and Liang He},
	      title     = {An Empirical Study of Personal Factors and Social Effects on Rating Prediction},
	      booktitle = {Proceedings of the 19th Pacific-Asia Conference on Knowledge Discovery and Data Mining},
	      pages     = {747-758},
	      year      = {2015},
	      series    = {PAKDD'15},
	    }
	    
	[15] Adaptive Friend Matrix Factorization, rating prediction, top-N recommendation.  
	    Adaptive Friend Biased Matrix Factorization, rating prediction, top-N recommendation.

### Content based filtering
	[0] Tag based filtering, tag based ItemkNN
	

### Evaluation metrics
	[1] error metrics: MAE, RMSE
	[2] accuracy metrics: Precision@N, Recall@N
	[3] ranking metrics: MAP@N
	[4] Others: Coverage, Popularity

### Data collections (examples)
	[1] Moive data: ml-100k, ml-1m, ml-10m
	[2] Social data: Douban, Epinions, Flixster
	[3] Location-based social network: Yelp

### Startup
	Vistual studio 2015. 
	Examples are allowed, please download the ml-100k data and configure its path.
	http://grouplens.org/datasets/movielens/
	
### Contact
	freepose@126.com

### License
	GPLv3.

### TODO
	[-] Factorization Machine